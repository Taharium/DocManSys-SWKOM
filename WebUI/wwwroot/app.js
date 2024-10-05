const apiUrl = 'http://localhost:8081/api/document';

function fillCard(doc){
    return `<div class="card m-2">
                <img src="${doc.image}" class="card-img-top" alt="${doc.title}" style="width: 150px; height: 150px">
                <div class="card-body bg-light">
                    <p class="m-0 " style="font-size: 12px">Title: ${doc.title}</p>
                    <p class="m-0" style="font-size: 12px">Author: ${doc.author}</p>
                    <button onclick="showDetails(${doc.id})" class="btn btn-primary mt-1 p-1" style="font-size: 12px">Details</button>
                    <button onclick="deleteDocument(${doc.id})" class="btn btn-danger mt-1 p-1" style="font-size: 12px">Delete</button>
                    <!--<a href="#" class="btn btn-primary">Go somewhere</a>-->
                </div>
            </div>`
    
    /*<span>Document: ${doc.id} | Title: ${doc.title}</span>
        <button class="btn btn-danger ms-2" onclick="deleteDocument(${doc.id})">Delete</button>
        <button class="btn btn-primary ms-2" onclick="toggleComplete(${doc.id}, ${doc.title}, '${doc.title}')">
            Mark as ${doc.title}
        </button> */
}

// Function to fetch and display Documents 
function fetchDocuments() {
    console.log('Fetching Documents items...');
    fetch(apiUrl)
        .then(response => 
            response.json()
        )
        .then(data => {
            const documentList = document.getElementById('documentList');
            documentList.innerHTML = ''; // Clear the list before appending new items
            data.forEach(doc => {
                const li = document.createElement('li');
                li.innerHTML = fillCard(doc);
                documentList.appendChild(li);
            });
        })
        .catch(error => console.error('Fehler beim Abrufen der Documents:', error));
}

// Function to add a new Document
function addDocument() {
    const title = document.getElementById('documentTitle').value;
    const author = document.getElementById('documentAuthor').value;
    const errorAutor = document.getElementById("errorAuthor");
    const errorTitle = document.getElementById("errorTitle");

    if (author.trim() === '' ) {
        errorAutor.innerHTML = "Please enter a Document Author";
        return;
    }
    
    if (title.trim() === '' ) {
        errorTitle.innerHTML = "Please enter a Document title";
        return;
    }
    
    errorTitle.innerHTML = ""
    errorAutor.innerHTML = ""
    author.value = ""
    title.value = ""
    
    const newDocument = {
        author: author,
        title: title,
        image: "Images/default_pdf.png"
    };
    
    console.log(JSON.stringify(newDocument))

    fetch(apiUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(newDocument)
    })
        .then(response => {
            console.log(response)
            if (response.ok) {
                window.location.href = "index.html"
                fetchDocuments(); // Refresh the list after adding
            } else {
                // Neues Handling für den Fall eines Fehlers (z.B. leeres Namensfeld)
                response.json().then(err => alert("Fehler: " + err.message));
                console.error('Fehler beim Hinzufügen der Aufgabe.');
            }
        })
        .catch(error => console.error('Fehler:', error));
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

// Function to toggle complete status
function toggleComplete(id, isComplete, name) {
    // Aufgabe mit umgekehrtem isComplete-Status aktualisieren
    const updatedDocument = {
        id: id,  // Die ID des Documents
        name: name, // Der Name des Documents
        isComplete: !isComplete // Status umkehren
    };

    fetch(`${apiUrl}/${id}`, {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(updatedDocument)
    })
        .then(response => {
            if (response.ok) {
                fetchDocuments(); // Liste nach dem Update aktualisieren
                console.log('Erfolgreich aktualisiert.');
            } else {
                console.error('Fehler beim Aktualisieren der Aufgabe.');
            }
        })
        .catch(error => console.error('Fehler:', error));
}

function searchDocument(){
    let text = document.getElementById("searchDoc").value;
    if(text === ""){
        fetchDocuments()
        return;
    }
    const searchUrl = `${apiUrl}?searchTerm=${encodeURIComponent(text)}`;
    
    fetch(searchUrl)
        .then(response =>
            response.json()
        )
        .then(data => {
            const documentList = document.getElementById('documentList');
            documentList.innerHTML = '';
            if(data.length === 0){
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

function showDetails(id){
    console.log(id)
}

function showAddDocument(){
    const a = document.getElementById("main")
    a.classList.add("d-none")
    document.getElementById("add").classList.remove("d-none")
}

function showMain(){
    const a = document.getElementById("add");
    a.classList.add("d-none");
    document.getElementById("searchDoc").value = '';
    document.getElementById("main").classList.remove("d-none");
    fetchDocuments();
}

// Load document items on page load
document.addEventListener('DOMContentLoaded', (event) => {
    let index = window.location.pathname;
    if(index.endsWith("index.html")){
        fetchDocuments();
    }
    /*document.getElementById("searchDocument").addEventListener("submit", function (e) {
        e.preventDefault();
    })*/
});