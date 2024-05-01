function onEditTap(id) {
    const cardId = `comment-${id}`;
    console.log(cardId);
    const commentCard = document.getElementById(`comment-${id}`);
    console.log(commentCard);
    const text = commentCard.querySelector('.comment-main-content');
    const form = `
        <textarea class="comment-edit-textarea" name="content">${text.innerText}</textarea>
        <div class="comment-edit-buttons">
            <button class="comment-edit-submit" type="sumbit">Submit</button>
            <button class="comment-edit-cancel" onclick="onCancelTap(${id})">Cancel</button>
        </div>
    `;
    
    const node = document.createElement('form');
    node.classList.add('comment-edit-form');
    node.method = 'POST';
    node.action = `/Comments/Edit/${id}`;
    node.innerHTML = form;
    text.replaceWith(node);
}

function onCancelTap(id) {
    const commentCard = document.getElementById(`comment-${id}`);
    const form = commentCard.querySelector('.comment-edit-form');
    const text = document.createElement('div');
    text.classList.add('comment-main-content');
    text.innerText = form.querySelector('textarea').value;
    form.replaceWith(text);
}