const apiUrl = 'http://localhost:8081/api/RestAPI/document';

function fillCard(doc){ //decodeURIComponent('${encodeURIComponent(JSON.stringify(doc))}')
    return `<div class="card m-2">
                <div class="d-flex card-body bg-light">
                    <div class="d-flex flex-column">
                        <div class="d-flex">
                            <strong class="align-self-start me-2">Author:</strong> <span>${doc.author}</span>
                        </div>
                        <div class="d-flex">
                            <strong class="align-self-start me-2">Title:</strong> <a href="#"  onclick="downloadFile('${doc.title}')"><div class="text-break">${doc.title}</div></a>
                        </div>
                    </div>
                    
            
                    <!-- Spacer to push buttons to the right -->
                    <div class="flex-grow-1 border-end me-3"></div>
            
                    <div class="">
                        <button onclick="showUpload(${doc.id})" class="btn btn-primary mt-1 p-1">Upload</button>
                        <button onclick="deleteDocument(${doc.id})" class="btn btn-danger mt-1 p-1">Delete</button>
                    </div>
                </div>
            </div>
`
}

// Function to fetch and display Documents 
function fetchDocuments() {
    const throbber = document.getElementById('throbber');
    throbber.style.display = 'block';

    console.log('Fetching Documents items...');
    fetch(apiUrl)
        .then(response =>
            response.json()
        )
        .then(data => {
            const documentList = document.getElementById('documentList');
            throbber.style.display = 'none';
            documentList.innerHTML = ''; // Clear the list before appending new items
            if (!data || data.length === 0) {
                const li = document.createElement('li');
                li.innerHTML = "No Documents";
                documentList.appendChild(li);
            }

            data.forEach(doc => {
                const li = document.createElement('li');
                li.innerHTML = fillCard(doc);
                documentList.appendChild(li);
                console.log(doc.ocrText)
            });
        })
        .catch(error => {
            throbber.style.display = 'none';
            console.error('Fehler beim Abrufen der Documents:', error)
        });
}

// Function to add a new Document
function addDocument() {
    const author = document.getElementById('documentAuthor');
    const errorDiv = document.getElementById('errorDiv');

    const newDocument = {
        author: author.value,
        title: "empty_doc.pdf",
    };
    
    fetch(apiUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(newDocument)
    })
        .then(response => {
            if (response.ok) {
                author.value = ""
                errorDiv.innerHTML = "";
                //window.location.href = "index.html"
                fetchDocuments(); // Refresh the list after adding
            } else {
                // Neues Handling für den Fall eines Fehlers (z.B. leeres Namensfeld)
                response.json().then(err => {
                    errorDiv.innerHTML = `<ul>` + Object.values(err.errors).map(e => `<li>${e}</li>`).join('') + `</ul>`
                });
            }
        })
        .catch(error => console.error('Fehler:', error));
    author.value = ""
}

// Function to delete a Document
function deleteDocument(id) {
    fetch(`${apiUrl}/${id}`, {
        method: 'DELETE'
    })
        .then(response => {
            if (response.ok) {
                fetchDocuments(); // Refresh the list after deletion
            } else {
                console.error('Fehler beim Löschen der Aufgabe.');
            }
        })
        .catch(error => console.error('Fehler:', error));
}

function searchDocument(){
    let text = document.getElementById("searchDoc").value;
    let selectedQuery = document.getElementById("dropdown").value;
    if(text === ""){
        fetchDocuments()
        return;
    }
    if(text.length < 3){
        return;
    }
    
    const searchUrl = `${apiUrl}/search/${selectedQuery}`;

    fetch(searchUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json' // Inform the server about the JSON payload
        },
        body: JSON.stringify(text) // Send the search term in the body
    })
        .then(response => {
            if(response.status === 200) {
                return response.json()
            }
        })
        .then(data => {
            const documentList = document.getElementById('documentList');
            documentList.innerHTML = '';
            if(!data || data.length === 0 || (data.message && typeof data.message === 'string')){
                const li = document.createElement('li');
                li.innerHTML = `<div class="h5">No Documents Found</div>`
                documentList.appendChild(li)
                return
            }
            data.forEach(doc => {
                const li = document.createElement('li');
                li.innerHTML = fillCard(doc);
                documentList.appendChild(li);
            });
        })
        .catch(error => console.error('Searching not working', error));
    
}

function downloadFile(fileName) {
    const fileUrl = `${apiUrl}/download/${fileName}`;

    const link = document.createElement('a');
    link.href = fileUrl;
    link.download = fileName; // Suggests a file name for saving
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

function showUpload(id) {
    const encodedId = encodeURIComponent(id);
    window.location = `uploadDocument.html?docId=${encodedId}`;
}

document.addEventListener('DOMContentLoaded', fetchDocuments );
