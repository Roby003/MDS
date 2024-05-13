window.addEventListener("load", () => {
  const boardsPreview = document.getElementsByClassName(
    "board-preview-wrapper"
  );
  for (let i = 0; i < boardsPreview.length; i++) {
    boardsPreview[i].addEventListener("click", () => {
      const boardId = boardsPreview[i].id;
      window.location.href = `/Boards/Show/${boardId}`;
    });
  }
});
