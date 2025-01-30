// js/Home/Index.js
document.addEventListener('DOMContentLoaded', () => {
    initializeAnimations();
    handleScrollEffects();
});

// Add to js/Home/Index.js

function isImageBloom(imageUrl) {
    if (!imageUrl) return false;
    try {
        const url = new URL(imageUrl);
        return url.protocol === 'http:' || url.protocol === 'https:';
    } catch {
        return false;
    }
}

// Add bloom card spacing and hover effects
document.addEventListener('DOMContentLoaded', () => {
    const bloomCards = document.querySelectorAll('.bloom-card');
    bloomCards.forEach((card, index) => {
        // Add alternating subtle background
        if (index % 2 === 0) {
            card.classList.add('bloom-card-alt');
        }

        // Add intersection observer for smooth appearance
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('bloom-visible');
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.1 });

        observer.observe(card);
    });
});

function initializeAnimations() {
    // Add subtle entrance animations to sections
    const sections = document.querySelectorAll('.glass-panel');
    sections.forEach((section, index) => {
        section.style.opacity = '0';
        section.style.transform = 'translateY(20px)';
        setTimeout(() => {
            section.style.transition = 'all 0.5s ease';
            section.style.opacity = '1';
            section.style.transform = 'translateY(0)';
        }, 100 * index);
    });
}

function handleScrollEffects() {
    const hero = document.querySelector('.home-hero');
    let lastScrollY = window.scrollY;

    window.addEventListener('scroll', () => {
        const currentScrollY = window.scrollY;

        // Parallax effect for hero section
        if (hero) {
            hero.style.backgroundPositionY = `${currentScrollY * 0.5}px`;
        }

        // Subtle scale effect for glass panels when they come into view
        const panels = document.querySelectorAll('.glass-panel');
        panels.forEach(panel => {
            const rect = panel.getBoundingClientRect();
            if (rect.top < window.innerHeight && rect.bottom > 0) {
                const distance = window.innerHeight - rect.top;
                const scale = Math.min(1 + (distance * 0.0001), 1.02);
                panel.style.transform = `scale(${scale})`;
            }
        });

        lastScrollY = currentScrollY;
    });
}