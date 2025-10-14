Response status: ${response.status}
Response headers:
<#list response.headers?keys as header>
${header}: ${response.headers[header]}
</#list>
Response body: ${response.body}