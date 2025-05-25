<h1>Отчёт по КР-2</h1>

<h2>1. Реализованные требования к функциональности</h2>
<ul>
  <li><code>Загрузка файлов</code> (<code>.txt</code>) с вычислением и проверкой хэша, а также проверкой на загрузку переданного файла ранее.</li>
  <li><code>Скачивание исходного файла</code> по хэшу.</li>
  <li><code>Анализ файла</code>: подсчёт параграфов, слов, символов, частот слов; генерация облака слов; сохранение PNG-изображения и JSON-ответа.</li>
  <li><code>Повторный запрос результата</code> без повторного анализа.</li>
  <li>Сохранение артефактов анализа в папке <code>storage</code>:<br>
    &nbsp;&nbsp;&bull; изображение <code>{id}.png</code><br>
    &nbsp;&nbsp;&bull; JSON-ответ <code>{id}.txt</code>
  </li>
  <li><code>Docker-Compose</code>: оркестрация всех контейнеров.</li>
  <li><code>Retry-механизм</code> и <code>healthcheck</code>.</li>
  <li><code>Swagger UI</code> для каждого микросервиса.</li>
</ul>

<h2>2. Распределение функциональности по микросервисам</h2>

<h3>2.1. Серверная часть каждого сервиса</h3>
<ul>
  <li><strong>API-Gateway</strong> (<code>:15000</code>): единая точка входа, маршрутизация запросов, HTTP-клиенты к остальным сервисам.</li>
  <li><strong>FileStoringService</strong> (<code>:15001</code>):<br>
    &nbsp;&nbsp;&bull; <code>POST /files/storage/upload</code> – загрузка, хеширование, сохранение на диск + БД <code>filestore</code>.<br>
    &nbsp;&nbsp;&bull; <code>GET /files/storage/files/{id}</code> – отдача бинарного файла.<br>
    &nbsp;&nbsp;&bull; EF Core, репозиторий, retry в старте для <code>EnsureCreated()</code>.
  </li>
  <li><strong>FileAnalysisService</strong> (<code>:15002</code>):<br>
    &nbsp;&nbsp;&bull; <code>POST /files/analysis/analyze/{id}</code> – полный конвейер анализа + вызов WordCloud.<br>
    &nbsp;&nbsp;&bull; <code>GET /files/analysis/results/{id}</code> – отдача ранее сохранённого DTO.<br>
    &nbsp;&nbsp;&bull; Сервисный слой, HTTP-адаптеры, EF Core <code>analysis</code>, retry.
  </li>
  <li><strong>WordCloudService</strong> (<code>:15003</code>):<br>
    &nbsp;&nbsp;&bull; <code>POST /wordcloud</code> – приём DTO со словами, формирование запроса к QuickChart, логирование входящего payload.
  </li>
</ul>

<h3>2.2. Обработка ошибок между микросервисами</h3>
<ul>
  <li><code>Retry-механизмы</code> при старте (до 10 попыток с паузой) для БД.</li>
  <li>HTTP-адаптеры с <code>EnsureSuccessStatusCode()</code> + <code>try/catch</code> + логирование.</li>
  <li>API-Gateway может перехватывать и возвращать <code>502 Bad Gateway</code> при ошибках downstream.</li>
  <li><code>Healthcheck</code> PostgreSQL гарантирует готовность базы до старта зависимых сервисов.</li>
</ul>

<h2>3. Swagger UI для микросервисов</h2>
<ul>
<li><a>API Gateway Swagger -  </a><a href="http://localhost:15000/swagger/index.html" target="_blank">http://localhost:15000/swagger/index.html</a></li>
  <li><a>FileStoringService Swagger - </a><a href="http://localhost:15001/swagger/index.html" target="_blank">http://localhost:15001/swagger/index.html</a></li>
  <li><a>FileAnalysisService Swagger - </a><a href="http://localhost:15002/swagger/index.html" target="_blank">http://localhost:15002/swagger/index.html</a></li>
  <li><a>WordCloudService Swagger - </a><a href="http://localhost:15003/swagger/index.html" target="_blank">http://localhost:15003/swagger/index.html</a></li>
</ul>

<h2>4. Архитектура всей системы</h2>
<ol>
  <li><strong>API-Gateway</strong> – фасад, маршрутизация, аутентификация/логирование.</li>
  <li><strong>FileStoringService</strong> – изолированное хранилище, БД <code>filestore</code>, диск.</li>
  <li><strong>FileAnalysisService</strong> – анализ, БД <code>analysis</code>, адаптеры к FileStoring и WordCloud.</li>
  <li><strong>WordCloudService</strong> – генерация облака слов через внешний API.</li>
  <li><strong>PostgreSQL</strong> – две изолированные БД, <code>healthcheck</code>.</li>
</ol>

<h2>5. Спецификация API</h2>
<table>
  <thead>
    <tr>
      <th>Сервис</th>
      <th>Метод</th>
      <th>Путь</th>
      <th>Описание</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>FileStoringService</td>
      <td>POST</td>
      <td><code>/files/storage/upload</code></td>
      <td>Загрузка файла и возврат метаданных</td>
    </tr>
    <tr>
      <td>FileStoringService</td>
      <td>GET</td>
      <td><code>/files/storage/files/{id}</code></td>
      <td>Скачивание загруженного файла</td>
    </tr>
    <tr>
      <td>FileAnalysisService</td>
      <td>POST</td>
      <td><code>/files/analysis/analyze/{id}</code></td>
      <td>Анализ файла и генерация артефактов</td>
    </tr>
    <tr>
      <td>FileAnalysisService</td>
      <td>GET</td>
      <td><code>/files/analysis/results/{id}</code></td>
      <td>Получение ранее сохранённого результата</td>
    </tr>
    <tr>
      <td>WordCloudService</td>
      <td>POST</td>
      <td><code>/wordcloud</code></td>
      <td>Генерация облака слов (<code>image/png</code>)</td>
    </tr>
  </tbody>
</table>

<h2>6. Чистота кода и лучшие практики</h2>
<ul>
  <li><strong>Трёхслойная архитектура</strong>: контроллеры → сервисы → репозитории/адаптеры.</li>
  <li><strong>Инверсия зависимостей</strong>: интерфейсы в Core/Common, реализации в Infrastructure.</li>
  <li><strong>Паттерн Adapter</strong> для HTTP-взаимодействия и доступа к БД.</li>
  <li><strong>Конфигурация</strong> через <code>IConfiguration</code> и переменные окружения.</li>
  <li><strong>Retry</strong> и <code>healthcheck</code>.</li>
  <li><strong>Swagger/OpenAPI</strong> – автодокументация API.</li>
  <li>Отдельные DTO/модели, чистые пакеты и слои.</li>
</ul>

<h2>7. Инструкция по использованию (CURL-запросы)</h2>

### 1. Загрузка файла
<pre>
  <code class="language-bash">
curl -v -X POST http://localhost:15000/files/storage/upload \
     -H "Content-Type: multipart/form-data" \
     -F "file=@[/path/to/local.txt]"
  </code>
</pre>

### 2. Скачивание файла
<pre>
  <code>
    curl -v http://localhost:15000/files/storage/files/[file_hash] \
     -o [/path/to/save.txt]
  </code>
</pre>

### 3. Анализ файла
<pre>
  <code>
  curl -v -X POST http://localhost:15000/files/analysis/analyze/[file_hash]
  </code>
</pre>

### 4. Получение ранее сохранённого результата
<pre>
  <code>
    curl -v http://localhost:15000/files/analysis/results/[file_hash]
  </code>
</pre>

<h2>8. UNIT-тестирование</h2>
Покрытие проекта тестами - 69%.
<p align="center">
 <img src=static/img/tests1.png width="400px">
</p>
<p align="center">
 <img src=static/img/tests2.png width="400px">
</p>

<h3>9. Скриншоты работы</h3>
<p align="center">
 <img src=static/img/screenshot1.png width="400px">
</p>
<p align="center">
 <img src=static/img/screenshot2.png width="400px">
</p>
<p align="center">
 <img src=static/img/screenshot3.png width="400px">
</p>
<p align="center">
 <img src=static/img/screenshot4.png width="400px">
</p>
<p align="center">
 <img src=static/img/screenshot5.png width="400px">
</p>
<p align="center">
 <img src=static/img/screenshot6.png width="400px">
</p>
<p align="center">
 <img src=static/img/screenshot7.png width="400px">
</p>
