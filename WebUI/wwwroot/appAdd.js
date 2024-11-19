const apiUrl = 'http://localhost:8081/api/RestAPI/document';


function addDocument() {
    const title = document.getElementById('documentTitle').files[0].name;
    const author = document.getElementById('documentAuthor').value;
    const errorDiv = document.getElementById('errorDiv');
    /*const errorAutor = document.getElementById("errorAuthor");
    const errorTitle = document.getElementById("errorTitle");

    let isValid = true
    if (author.trim() === '') {
        errorAutor.innerHTML = "Please enter a Document Author";
        isValid = false
    } else {
        errorAutor.innerHTML = ""
    }

    if (title.trim() === '') {
        errorTitle.innerHTML = "Please enter a Document Title";
        isValid = false
    } else {
        errorTitle.innerHTML = ""
    }

    if (!isValid) {
        return;
    }*/

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
                author.value = ""
                title.value = ""
                errorDiv.innerHTML = "";
                window.location.href = "index.html"
                //fetchDocuments(); // Refresh the list after adding
            } else {
                // Neues Handling für den Fall eines Fehlers (z.B. leeres Namensfeld)
                response.json().then(err => {
                    errorDiv.innerHTML = `<ul>` + Object.values(err.errors).map(e => `<li>${e}</li>`).join('') + `</ul>`
                });
            }
        })
        .catch(error => console.error('Fehler:', error));
}