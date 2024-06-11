window.addEventListener('load', () => {
    const preview = document.getElementsByClassName('edit-profile-picture-preview')[0];
    const fileInput = document.getElementsByClassName("edit-profile-picture-upload")[0];
    const form = document.getElementsByClassName("edit-profile-picture-form")[0];

    fileInput.addEventListener('change', () => {
        const file = fileInput.files[0];
        const reader = new FileReader();

        reader.onload = () => {
            preview.src = reader.result;
        }

        reader.readAsDataURL(file);
    });
});