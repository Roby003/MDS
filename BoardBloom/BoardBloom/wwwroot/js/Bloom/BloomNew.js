let image = undefined;
let title = undefined;
let content = undefined;

const openTab = (tabIndex) => {
    const tabs = document.getElementsByClassName('new-bloom-form');
    const tabButtons = document.getElementsByClassName('new-bloom-tabview-button');
    for (let i = 0; i < tabs.length; i++) {
        tabs[i].style.display = 'none';
        tabButtons[i].classList.remove('new-bloom-tabview-button-active');
    }

    tabs[tabIndex].style.display = 'flex';
    tabButtons[tabIndex].classList.add('new-bloom-tabview-button-active');
};

const backToEdit = () => {
    const tabview = document.getElementsByClassName('new-bloom-tabview')[0];
    tabview.style.display = null;

    const previewArea = document.getElementById('preview-area');
    previewArea.style.display = 'none';
}

const createPost = () => {
    const xhr = new XMLHttpRequest();

    xhr.open('POST', '/Blooms/NewBloom', true);

    xhr.setRequestHeader('Content-Type', 'application/json');

    const data = JSON.stringify({
        Title: title,
        Content: content,
        Image: image
    });

    xhr.onload = function() {
        if (xhr.status === 200) {
            const bloomId = JSON.parse(xhr.responseText)["id"];
            window.location.href = `/Blooms/Show/${bloomId}`;
        } else {
            console.error(xhr.responseText);
        }
    };

    xhr.onerror = function() {
        console.error('An error occurred during the request');
    };

    xhr.send(data);
}

const nextStep = () => {
    const activeForm = document.querySelector('.new-bloom-form[style="display: flex;"]');
    if (!activeForm) {
        console.error('No active form found');
        return;
    }

    title = activeForm.querySelector('input[name="title"]').value;
    content = activeForm.querySelector('textarea[name="content"]').value;
    image = activeForm.querySelector('input[name="image"]').value;

    const tabview = document.getElementsByClassName('new-bloom-tabview')[0];
    tabview.style.display = 'none';

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
                <div class="new-bloom-preview-buttons">
                    <button class="new-bloom-preview-back" onclick="backToEdit()">Back to edit</button>
                   
                    <button onclick="createPost()" class="new-bloom-preview-submit">Post</button>

                </div>
            `
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