// Debounce function to limit search API calls
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
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

    window.location.href = currentUrl.toString();
}, 500);

// Add smooth scroll animation for pagination
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

            // Navigate after scroll animation
            setTimeout(() => {
                window.location.href = href;
            }, 500);
        });
    });
});

// Add fade in animation for blooms when they come into view
document.addEventListener('DOMContentLoaded', () => {
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('fade-in');
            }
        });
    }, {
        threshold: 0.1
    });

    document.querySelectorAll('.bloom-card').forEach(bloom => {
        observer.observe(bloom);
    });
});