const apiUrl = 'http://localhost:8081/api/document';

// Function to fetch and display Documents 
function fetchDocuments() {
    console.log('Fetching Documents items...');
    fetch(apiUrl)
        .then(response => 
            response.json()
        )
        .then(data => {
            const documentList = document.getElementById('documentList');
            console.log(data)
            documentList.innerHTML = ''; // Clear the list before appending new items
            data.forEach(doc => {
                console.log(doc)
                // Create list item with delete and toggle complete buttons
                const li = document.createElement('li');
                li.innerHTML = `
                    <div class="card w-25 ">
                        <img src="${doc.image}" class="card-img-top" alt="${doc.title}">
                        <div class="card-body">
                            <h5 class="card-title">${doc.title}</h5>
                            <p class="card-text">Written by the Author: ${doc.author}</p>
                            <!--<a href="#" class="btn btn-primary">Go somewhere</a>-->
                        </div>
                    </div>
                    <!--<span>Document: ${doc.id} | Title: ${doc.title}</span>
                    <button class="btn btn-danger ms-2" onclick="deleteDocument(${doc.id})">Delete</button>
                    <button class="btn btn-primary ms-2" onclick="toggleComplete(${doc.id}, ${doc.title}, '${doc.title}')">
                        Mark as ${doc.title}
                    </button> -->
                `;
                documentList.appendChild(li);
                
            });
        })
        .catch(error => console.error('Fehler beim Abrufen der Documents:', error));
}

// Function to add a new Document
function addDocument() {
    const documentName = document.getElementById('DocumentName').value;
    const isComplete = document.getElementById('isComplete').checked;

    if (documentName.trim() === '') {
        alert('Please enter a Document name');
        return;
    }

    const newDocument = {
        name: documentName,
        isComplete: isComplete
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
                fetchDocuments(); // Refresh the list after adding
                document.getElementById('DocumentName').value = ''; // Clear the input field
                document.getElementById('isComplete').checked = false; // Reset checkbox
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

function showAddDocument(){
    const a = document.getElementById("main")
    a.classList.add("d-none")
    document.getElementById("add").classList.remove("d-none")
}

function showMain(){
    const a = document.getElementById("add")
    a.classList.add("d-none")
    document.getElementById("main").classList.remove("d-none")
    fetchDocuments()
}

// Load document items on page load
document.addEventListener('DOMContentLoaded', (event) => {
    fetchDocuments();
    document.getElementById("searchDocument").addEventListener("submit", function (e) {
        e.preventDefault();
    })
});