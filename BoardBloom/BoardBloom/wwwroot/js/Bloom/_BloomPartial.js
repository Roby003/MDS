let cachedBoards = undefined;

async function onSaveButtonClick(bloomId, userId) {
    const bloom = document.getElementById(bloomId);
    const saveMenu = bloom.getElementsByClassName('bloom-save-options')[0];
    saveMenu.innerHTML = 'Loading...';


    const link = `/Boards/GetAllUserBoards?userId=${userId}&bloomId=${bloomId}`;
    const boards = await fetch(link)
        .then(response => response.json())
        .then(data => {
            return data
                .map((item => {
                    const board = item.board;
                    return {
                        id: board.id,
                        name: board.name,
                        noOfPosts: item.bloomsCount,
                        saved: item.saved
                    };
                }));
        });
    
    saveMenu.innerHTML = '';
    boards.forEach(board => {
        const boardElement = document.createElement('div');
        boardElement.classList.add('board-save-option');
        boardElement.innerHTML = `
            <label class="bloom-save-option">
            <input type="checkbox" name="board" value="${board.id}" ${board.saved ? 'checked' : ''}>
            <span>${board.name} (${board.noOfPosts})</span>
            </label>`;
        saveMenu.appendChild(boardElement);
    });
    cachedBoards = boards;
};

function cancel(bloomId) {
    const saveMenu = document.getElementsByClassName('bloom-save-options')[0];
    saveMenu.innerHTML = '';
}

async function handleSaveSubmit(bloomId) {
    const checkedBoards = document.querySelectorAll('.board-save-option input[type="checkbox"]');
    let changedBoards = [];
    cachedBoards.forEach((board, index) => {
        const checkbox = checkedBoards[index];
        if (board.saved !== checkbox.checked) {
            changedBoards.push(checkbox);
        }
    });
    const checkedBoardsIds = Array.from(changedBoards).map(checkbox => checkbox.value);
    const formatted = checkedBoardsIds.join(',');

    const link = `/Boards/AddBloomToBoards/${bloomId}?boardsIds=${formatted}`;
    const response = await fetch(link, {
        method: 'POST'
    });

    if (response.ok) {
        alert('Bloom saved successfully!');
    } else {
        alert('An error occurred while saving the bloom!');
    }
}