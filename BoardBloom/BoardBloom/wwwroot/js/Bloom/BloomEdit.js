let image = undefined;
let title = undefined;
let content = undefined;
let id = undefined;

const backToEdit = () => {
    const form = document.getElementsByClassName('edit-bloom-forms-wrapper')[0];
    form.style.display = 'flex';

    const previewArea = document.getElementById('preview-area');
    previewArea.style.display = 'none';
}

const nextStep = () => {

    title = document.querySelector('input[name="title"]').value;
    content = document.querySelector('textarea[name="content"]').value;
    image = document.querySelector('input[name="image"]').value;
    id = document.querySelector('input[name="id"]').value;

    const newBloomForm = document.getElementsByClassName('edit-bloom-forms-wrapper')[0];
    newBloomForm.style.display = 'none';

    const xhr = new XMLHttpRequest();

    xhr.open('POST', '/Blooms/Preview', true);

    xhr.setRequestHeader('Content-Type', 'application/json');

    const data = JSON.stringify({
        Title: title,
        Content: content,
        Image: image
    });

    xhr.onload = function() {
        if (xhr.status === 200) {
           
            const html = JSON.parse(xhr.responseText)["html"];
            const previewArea = document.getElementById('preview-area')
            previewArea.innerHTML = html;

            const previewForm = `
                <div class="edit-bloom-preview-buttons">
                    <button class="edit-bloom-preview-back" onclick="backToEdit()">Back to edit</button>
                   
                    <form class="edit-bloom-preview-form" method="POST" action="/Blooms/Edit/${id}">
                        <input type="hidden" name="title" value="${title}">
                        <input type="hidden" name="content" value="${content}">
                        <input type="hidden" name="image" value="${image}">
                        <button type="submit" class="edit-bloom-preview-submit">Save changes</button>
                    </form>

                </div>
            `;
            previewArea.innerHTML += previewForm;

            previewArea.style.display = null;
        } else {
            console.error(xhr.responseText);
        }
    };

    xhr.onerror = function() {
        console.error('An error occurred during the request');
    };

    xhr.send(data);
};