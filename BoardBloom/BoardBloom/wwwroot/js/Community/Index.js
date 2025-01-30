let searchTimeout = null;

function showLoading() {
    document.getElementById('loadingState').style.display = 'flex';
}

function hideLoading() {
    document.getElementById('loadingState').style.display = 'none';
}

function handleInputChange() {
    const searchInput = document.getElementById('searchInput');
    const searchTerm = searchInput.value;

    // Clear existing timeout
    if (searchTimeout) {
        clearTimeout(searchTimeout);
    }

    // Set new timeout to avoid too many requests
    searchTimeout = setTimeout(() => {
        fetchCommunities(searchTerm);
    }, 300);
}

async function fetchCommunities(searchTerm) {
    try {
        showLoading();
        const response = await fetch(`/Communities/GetCommunitiesByName?name=${encodeURIComponent(searchTerm)}`);
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        const html = await response.text();
        document.getElementById('indexWrapper').innerHTML = html;
    } catch (error) {
        console.error('Error fetching communities:', error);
        document.getElementById('indexWrapper').innerHTML = `
            <div class="empty-state">
                <div class="empty-state-icon">⚠️</div>
                <h3 class="empty-state-title">Oops! Something went wrong</h3>
                <p class="empty-state-description">Please try again later</p>
            </div>`;
    } finally {
        hideLoading();
    }
}