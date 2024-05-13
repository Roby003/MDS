async function onSaveButtonClick(bloomId, userId) {
    const bloom = document.getElementById(bloomId);
    const saveMenu = bloom.getElementsByClassName('bloom-save-options')[0];
    saveMenu.innerHTML = 'Loading...';


    const link = `/Boards/GetAllUserBoards?userId=${userId}`;
    const boards = await fetch(link)
        .then(response => response.json())
        .then(data => {
            return data.$values
                .filter(board => board.UserId)
                .map(board => {
                    return {
                        id: board.Id,
                        name: board.Name,
                        noOfPosts: board.BloomBoards.$values.length
                    };
                });
        });
    console.log(boards);
    
    console.log(saveMenu);
    saveMenu.innerHTML = '';
    boards.forEach(board => {
        const boardElement = document.createElement('div');
        boardElement.classList.add('board-save-option');
        boardElement.innerHTML = `
            <label class="bloom-save-option">
                <input type="checkbox" name="board" value="${board.id}">
                <span>${board.name} (${board.noOfPosts})</span>
            </label>`;
        saveMenu.appendChild(boardElement);
    });
};