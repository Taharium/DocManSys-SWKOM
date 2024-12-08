const apiUrl = 'http://localhost:8081/api/RestAPI/document';

function AddUploadInput(doc){
    return `
            <!-- Title Field -->
            <div class="mb-3">
                <label for="uploadDocumentTitle" class="form-label">Title</label>
                <input type="file" class="form-control" id="uploadDocumentTitle" accept="application/pdf">
                <span class="d-flex align-self-start">Previous Document: ${doc.title}</span>
            </div>
            
            <div class="d-grid">
                <button onclick="uploadDocument(${doc.id})" class="btn btn-primary">Upload Document</button>
            </div>
            `
}

function uploadDocument(id){
    const fileInput = document.getElementById("uploadDocumentTitle");
    uploadFile(id, fileInput);
}

function uploadFile(documentId, fileInput) {
    let file = "";
    const errorDiv = document.getElementById('errorDiv');
    console.log(fileInput.files)
    if(fileInput.files === null || fileInput.files.length === 0){
        errorDiv.innerHTML = `<li>No File chosen.</li>`
        return;
    }
    file = fileInput.files[0];

    const formData = new FormData();
    formData.append('documentFile', file);
    console.log(formData);

    fetch(`${apiUrl}/${documentId}/upload`, {
        method: 'PUT',
        body: formData
    })
        .then(response => {
            if (response.ok) {
                window.location = "index.html"
                errorDiv.innerHTML = '';
            } else {
                response.json().then(err => {
                    if (err.errors) {
                        errorDiv.innerHTML = `<span>` + Object.values(err.errors).map(e => `<li>${e}</li>`).join('') + `</span>`;
                    } else {
                        errorDiv.innerHTML = `<li>Error while uploading File.</li>`;
                    }
                }).catch(() => {
                    errorDiv.innerHTML = `<li>Error while uploading File.</li>`;
                });
            }
        })
        .catch(error => {
            console.error('Error:', error);
        });
}

function extractId(){
    const urlParams = new URLSearchParams(window.location.search);
    const docId = urlParams.get('docId');
    const docForm =  document.getElementById('uploadDocumentForm');
    docForm.innerHTML = '';
    if(!docId){
        const li = document.createElement('li');
        li.innerHTML = `<div class="h5">Document not Found</div>`;
        docForm.appendChild(li);
        return;
    }
    
    const idUrl = `${apiUrl}/${docId}`;

    fetch(idUrl)
        .then(response =>
            response.json()
        )
        .then(doc => {
            docForm.innerHTML = '';
            const li = document.createElement('li');
            li.innerHTML = AddUploadInput(doc);
            docForm.appendChild(li);
        })
        .catch(error => console.error('No Document found', error));

}

document.addEventListener('DOMContentLoaded',  extractId);
