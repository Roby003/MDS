async function deleteProfilePicture(userId) {
    const url = `/Users/DeleteProfilePicture/${userId}`;
    await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        if (response.ok) {
            const profilePicture = document.getElementsByClassName('user-profile-picture')[0];
            profilePicture.src =
                "https://st3.depositphotos.com/6672868/13701/v/450/depositphotos_137014128-stock-illustration-user-profile-icon.jpg";
        }
    });
}