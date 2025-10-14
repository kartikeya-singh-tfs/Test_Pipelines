Request method: ${request.method}
Request URI: ${request.uri}
Request headers:
<#list request.headers?keys as header>
${header}: ${request.headers[header]}
</#list>
Request body: ${request.body}