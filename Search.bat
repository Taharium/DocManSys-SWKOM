@echo off
:: add document
curl --location "http://localhost:8081/api/RESTAPI/Document" --header "Content-Type: application/json" --data "{\"title\": \"Sample Document\", \"author\": \"Test\", \"ocrText\": \"This is an example OCR text.\"}" | jq -r .id > temp1_id.txt
set /p DOCID=<temp1_id.txt

:: add file
curl --location --request PUT "http://localhost:8081/api/RESTAPI/Document/%DOCID%/upload" --form "documentFile=@uploads\\Proposal Draft.pdf"

pause

:: fuzzy search
echo.
echo searches with fuzzy search
curl --location "http://localhost:8081/api/RESTAPI/Document/search/fuzzy" --header "Content-Type: application/json" --data "\"Moulahi\"" | jq

pause 

:: wildcard search
echo searches with query string
curl --location "http://localhost:8081/api/RESTAPI/Document/search/querystring" --header "Content-Type: application/json" --data "\"Felgitsch\"" | jq

pause 

:: search but no content because no document has this content
echo search but no content because no document has this content
curl --location "http://localhost:8081/api/RESTAPI/Document/search/fuzzy" --header "Content-Type: application/json" --data "\"spikgjhwseapiojgiopaswej\"" --write-out "%%{http_code}" --silent --output response.json > status_code.txt

pause

set /p status1_code=<status_code.txt
if "%status1_code%" == "204" (
    echo Curl Script output: No Document with this content.
) else (
    echo Curl Script output: Something went wrong in the curl script!!!!
)

:: delete document
echo deletes document
curl --location --request DELETE "http://localhost:8081/api/RESTAPI/Document/%DOCID%" --write-out "%%{http_code}" --silent --output response.json > status_code.txt

set /p status_code=<status_code.txt

if "%status_code%" == "204" (
    echo Curl Script output: Document successfully deleted.
) else (
    echo Curl Script output: Something went wrong in the curl script!!!!
)


:: deleting same document again (should fail)
echo tries to delete same document as before (should fail)
curl --location --request DELETE "http://localhost:8081/api/RESTAPI/Document/%DOCID%"

del status_code.txt
del response.json
del temp1_id.txt

