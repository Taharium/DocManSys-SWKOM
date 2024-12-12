@echo off

echo "Adding document should fail (empty title)"

curl -s --location http://localhost:8081/api/RESTAPI/Document --header "Content-Type: application/json" --data "{\"title\": \"\", \"author\": \"Doe\", \"ocrText\": \"This is an example OCR text.\"}" | jq -r .errors

echo "Adding document should fail (empty author)"

curl -s --location http://localhost:8081/api/RESTAPI/Document --header "Content-Type: application/json" --data "{\"title\": \"Test\", \"author\": \"\", \"ocrText\": \"This is an example OCR text.\"}" | jq -r .errors


echo "Adding a Document Object to the database successfully"

curl -s --location http://localhost:8081/api/RESTAPI/Document --header "Content-Type: application/json" --data "{\"title\": \"test.pdf\", \"author\": \"Doe\", \"ocrText\": \"This is an example OCR text.\"}" | jq -r .id > temp_id.txt
set /p DOCID=<temp_id.txt
echo The document ID is: %DOCID%

echo "Adding document should fail (existing id)"

curl -s --location http://localhost:8081/api/RESTAPI/Document --header "Content-Type: application/json" --data "{\"id\": \"1\", \"title\": \"Test\", \"author\": \"Doe\", \"ocrText\": \"This is an example OCR text.\"}" | jq .message

echo "Uploading document should fail (not existing id)"
set /a WRONGID=%DOCID% + 1
echo %WRONGID%
curl --location --request PUT "http://localhost:8081/api/RESTAPI/Document/%WRONGID%/upload" --header "Content-Type: multipart/form-data" --form "documentFile=@uploads\\empty_doc.pdf"

echo "Uploading document successfully"

curl --location --request PUT "http://localhost:8081/api/RESTAPI/Document/%DOCID%/upload" --header "Content-Type: multipart/form-data" --form "documentFile=@uploads\\empty_doc.pdf"

echo "Changing uploaded document successfully"

curl --location --request PUT "http://localhost:8081/api/RESTAPI/Document/%DOCID%/upload" --header "Content-Type: multipart/form-data" --form "documentFile=@uploads\\Level_Analysis_Moulahi.pdf"

