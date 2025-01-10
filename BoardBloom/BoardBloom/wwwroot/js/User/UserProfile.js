document.addEventListener('DOMContentLoaded', function () {
    initializeTabs();
    initializeProfileBanner();
});

function initializeTabs() {
    const tabButtons = document.querySelectorAll('.tab-button');
    const tabContents = document.querySelectorAll('.tab-content');

    // Set initial active tab
    tabButtons[0].classList.add('active');
    tabContents[0].classList.add('active');

    tabButtons.forEach(button => {
        button.addEventListener('click', () => {
            const targetTab = button.getAttribute('data-tab');

            // Remove active class from all buttons and contents
            tabButtons.forEach(btn => btn.classList.remove('active'));
            tabContents.forEach(content => content.classList.remove('active'));

            // Add active class to clicked button
            button.classList.add('active');

            // Add active class to corresponding content
            const targetContent = document.getElementById(targetTab + '-content');
            if (targetContent) {
                targetContent.classList.add('active');
            }
        });
    });
}
async function deleteProfilePicture(userId) {
    if (!confirm('Are you sure you want to delete your profile picture?')) {
        return;
    }

    try {
        const response = await fetch(`/Users/DeleteProfilePicture/${userId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            window.location.reload();
        } else {
            alert('Failed to delete profile picture. Please try again.');
        }
    } catch (error) {
        console.error('Error:', error);
        alert('An error occurred while deleting the profile picture.');
    }
}