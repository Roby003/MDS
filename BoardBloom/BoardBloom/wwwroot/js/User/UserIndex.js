// Debounce function for search
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args)

{
    const later = () =>

{
    clearTimeout(timeout);
    func(...args);
}

;
clearTimeout(timeout);
timeout = setTimeout(later, wait);
}
;
}

// Handle search input
const handleSearch = debounce((event) => {
    const searchQuery = event.target.value.trim();
    const currentUrl = new URL(window.location.href);
    
    if (searchQuery) {
        currentUrl.searchParams.set('search', searchQuery);
        currentUrl.searchParams.set('page', '1');
    } else {
        currentUrl.searchParams.delete('search');
    }
    
    // Show loading state
    showLoading();
    
    window.location.href = currentUrl.toString();
}, 500);

// Loading state management
function showLoading() {
    const overlay = document.createElement('div');
    overlay .className = 'loading-overlay';
    overlay .innerHTML = '<div class="loading-spinner"></div>';
    document .body.appendChild(overlay);
}

// Smooth scroll for pagination
document.addEventListener('DOMContentLoaded', () => {
    const paginationLinks = document.querySelectorAll('.page-link');
    
    paginationLinks.forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            const href = e.currentTarget.getAttribute('href');
            
            // Smooth scroll to top before navigating
            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            });
            
            // Show loading state
            showLoading();
            
            // Navigate after scroll animation
            setTimeout(() => {
                window.location.href = href;
            }, 500);
        });
    });
});

// Lazy load user profile images
document.addEventListener('DOMContentLoaded', () => {
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('fade-in');
                
                // Load profile image if it exists
                const img = entry.target.querySelector('.user-profile-image');
                if (img && img.dataset.src) {
                    img.src = img.dataset.src;
                    img.removeAttribute('data-src');
                }
            }
        });
    }, {
        threshold: 0.1
    });

    document.querySelectorAll('.user-card').forEach(card => {
        observer.observe(card);
    });
});

// Handle delete user confirmation
document.addEventListener('DOMContentLoaded', () => {
    const deleteButtons = document.querySelectorAll('.delete-user-button');
    
    deleteButtons.forEach(button => {
        button.addEventListener('click', (e) => {
            if (!confirm('Are you sure you want to delete this user? This action cannot be undone.')) {
                e.preventDefault();
            }
        });
    });
});
