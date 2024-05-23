function onEditTap(id) {

    // Close any open edit boxes before opening a new one
    const openEditForms = document.querySelectorAll('.comment-edit-form');
    openEditForms.forEach(form => {
        const originalCommentId = form.closest('.comment-card').id.split('-')[1];
        onCancelTap(originalCommentId);
    });

    //-------
    const cardId = `comment-${id}`;
    console.log(cardId);
    const commentCard = document.getElementById(`comment-${id}`);
    console.log(commentCard);
    const text = commentCard.querySelector('.comment-main-content');
    const form = `
        <textarea class="comment-edit-textarea" name="content">${text.innerText}</textarea>
        <div class="comment-edit-buttons">
            <div class="comment-edit-submit" onclick="handleCommEdit(event,${id})">Submit</div>
            <div class="comment-edit-cancel" onclick="onCancelTap(${id})">Cancel</div>
        </div>
    `;
    
    const node = document.createElement('form');
    node.classList.add('comment-edit-form');
    node.method = 'POST';
    node.action = `/Comments/Edit?id=${id}`;
    node.innerHTML = form;
    text.replaceWith(node);
}
async function handleCommEdit(event, commId) {
    event.preventDefault();
    const form = document.querySelector('.comment-edit-form');
    const actionUrl = form.action;

    console.log(actionUrl)
    const dataToSend = new FormData(form);
    const response = await fetch(actionUrl, {
        method: 'POST',
        body: dataToSend
    });
    const data = await response.text();

    const commentCard = document.getElementById(`comment-${commId}`);
    commentCard.innerHTML = data;

}
async function onCancelTap(id) {
    //cancel no longer updates the comment
    console.log(id)
    const commentCard = document.getElementById(`comment-${id}`);

    const url = `/Comments/GetComm?id=${id}`
    const response = await fetch(url);
    const data = await response.text();
    commentCard.innerHTML = data;

}