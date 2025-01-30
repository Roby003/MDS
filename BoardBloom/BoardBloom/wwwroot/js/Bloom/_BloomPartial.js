/*let cachedBoards = undefined;*/


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
    if (boards.isEmpty) {
        const noBoardsElement = document.createElement('div');
        noBoardsElement.classList.add('bloom-save-option');
        noBoardsElement.innerHTML = 'No boards found';
        saveMenu.appendChild(noBoardsElement);
    } else {
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
    }
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

async function checkIfLiked() {
    const blooms = document.getElementsByClassName("bloom-card");
    for (let i = 0; i < blooms.length; i++) {
        const bloom = blooms[i];
        const id = bloom.id;
        var url = `Blooms/CheckLike?bloomId=${id}`
        const bloomFooterLike = bloom.getElementsByClassName("bloom-footer-like")[0].getElementsByTagName("img")[0];
        await fetch(url).then(response => response.json()).then(data => {
            var redHeartsrc = "https://upload.wikimedia.org/wikipedia/commons/thumb/3/32/WikiFont_uniE033_-_heart_-_red.svg/800px-WikiFont_uniE033_-_heart_-_red.svg.png"
            if (data.liked === true)
                bloomFooterLike.src = redHeartsrc;

        })
    }
}
async function handleLikeClick(event, bloomId) {
    event.preventDefault();
/*    console.log(bloomId);
*/    event.stopPropagation();
    var url = `/Blooms/Like?bloomId=${bloomId}`
    var id = `likeCount_${bloomId}`
    /* console.log(id);*/
    await fetch(url, { method: 'POST' }).then(response => response.json()).then(data => {
        document.getElementById(id).textContent = data.likeCount;
        var redHeartsrc = "https://upload.wikimedia.org/wikipedia/commons/thumb/3/32/WikiFont_uniE033_-_heart_-_red.svg/800px-WikiFont_uniE033_-_heart_-_red.svg.png"
        var whiteHeartsrc = "https://icons.veryicon.com/png/o/miscellaneous/ui-basic-linear-icon/like-106.png"
        var heartIcon = document.getElementById(`likeIcon_${bloomId}`)


        if (data.userLikedPost == true) {
            heartIcon.src = redHeartsrc;
        } else {
            heartIcon.src = whiteHeartsrc;
        }

    })
}

window.addEventListener('load', checkIfLiked());