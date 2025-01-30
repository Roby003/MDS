let state = {
    image: '',
    title: '',
    content: ''
};

const openTab = (tabIndex) => {
    const tabs = document.getElementsByClassName('new-bloom-form');
    const tabButtons = document.getElementsByClassName('new-bloom-tabview-button');

    Array.from(tabs).forEach((tab, i) => {
        tab.style.display = 'none';
        tabButtons[i].classList.remove('new-bloom-tabview-button-active');
    });

    tabs[tabIndex].style.display = 'flex';
    tabButtons[tabIndex].classList.add('new-bloom-tabview-button-active');
};

const getFormData = () => {
    // Get the active form
    const activeForm = document.querySelector('.new-bloom-form[style="display: flex;"]');
    if (!activeForm) {
        throw new Error('No active form found');
    }

    // Check if it's an image post or text post
    const isImagePost = activeForm.querySelector('input[name="image"]') !== null;

    if (isImagePost) {
        return {
            Title: 'Image Post', // Default title for image posts
            Content: activeForm.querySelector('textarea[name="content"]').value,
            Image: activeForm.querySelector('input[name="image"]').value
        };
    } else {
        return {
            Title: activeForm.querySelector('input[name="title"]').value,
            Content: activeForm.querySelector('textarea[name="content"]').value,
            Image: '' // No image for text posts
        };
    }
};

const validateForm = (data) => {
    const activeForm = document.querySelector('.new-bloom-form[style="display: flex;"]');
    const isImagePost = activeForm.querySelector('input[name="image"]') !== null;

    if (isImagePost) {
        if (!data.Image || !data.Content) {
            throw new Error('Please fill in both image URL and content');
        }
    } else {
        if (!data.Title || !data.Content) {
            throw new Error('Please fill in both title and content');
        }
    }
};

const createPost = async (communityId) => {
    try {
        // Debug logging
        console.log('Creating post with communityId:', communityId);
        console.log('Current state:', state);

        const path = (communityId && communityId !== 'null' && communityId !== 'undefined')
            ? `/Blooms/NewBloom?communityId=${communityId}`
            : '/Blooms/NewBloom';

        console.log('Request path:', path);

        const response = await fetch(path, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify({
                ...state,
                CommunityId: communityId // Explicitly include communityId in the request body
            })
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Failed to create post');
        }

        const data = await response.json();
        console.log('Server response:', data);

        if (data.bloomId) {
            const redirectUrl = data.communityId
                ? `/Communities/Show/${data.communityId}`
                : `/Blooms/Show/${data.bloomId}`;

            console.log('Redirecting to:', redirectUrl);
            window.location.href = redirectUrl;
        } else {
            throw new Error('No bloom ID returned from server');
        }
    } catch (error) {
        console.error('Error creating post:', error);
        alert(error.message || 'Failed to create post. Please try again.');
    }
};

const nextStep = async (communityId) => {
    try {
        // Convert empty string or 'undefined' to null
        communityId = (communityId && communityId !== 'undefined' && communityId !== '')
            ? parseInt(communityId)
            : null;

        console.log('NextStep called with communityId:', communityId); // Debug log

        // Get and validate form data
        const formData = getFormData();
        validateForm(formData);

        // Update state
        state = {
            ...formData,
            CommunityId: communityId
        };

        const tabview = document.querySelector('.new-bloom-tabview');
        if (tabview) {
            tabview.style.display = 'none';
        }

        // Preview request
        const response = await fetch('/Blooms/Preview', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify(formData)
        });

        if (!response.ok) {
            throw new Error('Failed to generate preview');
        }

        const data = await response.json();
        const previewArea = document.getElementById('preview-area');

        if (previewArea) {
            previewArea.innerHTML = data.html;

            const previewButtons = `
                <div class="new-bloom-preview-buttons">
                    <button class="new-bloom-preview-back" onclick="backToEdit()">Back to edit</button>
                    <button onclick="createPost(${communityId || null})" class="new-bloom-preview-submit">Post</button>
                </div>
            `;

            previewArea.innerHTML += previewButtons;
            previewArea.style.display = '';
        }
    } catch (error) {
        console.error('Error in preview:', error);
        alert(error.message || 'Failed to generate preview. Please try again.');
    }
};

const backToEdit = () => {
    const tabview = document.querySelector('.new-bloom-tabview');
    const previewArea = document.getElementById('preview-area');

    if (tabview && previewArea) {
        tabview.style.display = '';
        previewArea.style.display = 'none';
    }
};