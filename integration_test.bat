@echo off

echo.
echo "Adding document should fail (empty title)"

curl -s --location http://localhost:8081/api/RESTAPI/Document --header "Content-Type: application/json" --data "{\"title\": \"\", \"author\": \"Doe\", \"ocrText\": \"This is an example OCR text.\"}" | jq -r .errors

pause

echo.
echo "Adding document should fail (empty author)"

curl -s --location http://localhost:8081/api/RESTAPI/Document --header "Content-Type: application/json" --data "{\"title\": \"Test\", \"author\": \"\", \"ocrText\": \"This is an example OCR text.\"}" | jq -r .errors

pause

echo.
echo "Adding a Document Object to the database successfully"

curl -s --location http://localhost:8081/api/RESTAPI/Document --header "Content-Type: application/json" --data "{\"title\": \"test.pdf\", \"author\": \"Doe\", \"ocrText\": \"This is an example OCR text.\"}" | jq -r .id > temp_id.txt
set /p DOCID=<temp_id.txt
echo The document ID is: %DOCID%

pause

echo.
echo "Adding document should fail (existing id)"

curl -s --location http://localhost:8081/api/RESTAPI/Document --header "Content-Type: application/json" --data "{\"id\": \"%DOCID%\", \"title\": \"Test\", \"author\": \"Doe\", \"ocrText\": \"This is an example OCR text.\"}" | jq .message

pause

echo.
echo "Uploading document should fail (not existing id)"
set /a WRONGID=%DOCID% + 1
echo %WRONGID%
curl --location --request PUT "http://localhost:8081/api/RESTAPI/Document/%WRONGID%/upload" --header "Content-Type: multipart/form-data" --form "documentFile=@uploads\\empty_doc.pdf" | jq  .message

echo.
pause

echo.
echo "Uploading document successfully"

curl --location --request PUT "http://localhost:8081/api/RESTAPI/Document/%DOCID%/upload" --header "Content-Type: multipart/form-data" --form "documentFile=@uploads\\empty_doc.pdf" | jq  .message

echo.
pause

echo.
echo "Changing uploaded document successfully"

curl --location --request PUT "http://localhost:8081/api/RESTAPI/Document/%DOCID%/upload" --header "Content-Type: multipart/form-data" --form "documentFile=@uploads\\Level_Analysis_Moulahi.pdf" | jq .message

echo.
pause

:: fuzzy search
echo.
echo searches with fuzzy search
curl --location "http://localhost:8081/api/RESTAPI/Document/search/fuzzy" --header "Content-Type: application/json" --data "\"Moulahi\"" | jq 

echo.
pause 

:: wildcard search
echo.
echo searches with query string
curl --location "http://localhost:8081/api/RESTAPI/Document/search/querystring" --header "Content-Type: application/json" --data "\"Taha\"" | jq 

echo.
pause 

:: search but no content because no document has this content
echo.
echo "searching for content that is not in the document (spikgjhwseapiojgiopaswej)"
curl --location "http://localhost:8081/api/RESTAPI/Document/search/fuzzy" --header "Content-Type: application/json" --data "\"spikgjhwseapiojgiopaswej\"" --write-out "%%{http_code}" --silent --output response.json > status_code.txt


set /p status1_code=<status_code.txt
if "%status1_code%" == "204" (
    echo "Curl Script output: No Document with this content (spikgjhwseapiojgiopaswej)."
) else (
    echo Curl Script output: Something went wrong in the curl script!!!!
)

echo.
pause

:: delete document
echo.
echo deletes document
curl --location --request DELETE "http://localhost:8081/api/RESTAPI/Document/%DOCID%" --write-out "%%{http_code}" --silent --output response.json > status_code.txt

set /p status_code=<status_code.txt

if "%status_code%" == "204" (
    echo Curl Script output: Document successfully deleted.
) else (
    echo Curl Script output: Something went wrong in the curl script!!!!
)

echo.
pause

:: deleting same document again (should fail)
echo.
echo tries to delete same document as before (should fail)
curl --location --request DELETE "http://localhost:8081/api/RESTAPI/Document/%DOCID%"
echo.

del status_code.txt
del response.json
del temp_id.txt

pause