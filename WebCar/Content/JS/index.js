// ========== CAR CAROUSEL CONTROLS ==========
document.addEventListener('DOMContentLoaded', function () {
    const carousel = document.getElementById('carousel');
    if (!carousel) return;

    const track = carousel.querySelector('.slide-track');
    const slides = carousel.querySelectorAll('.slide');
    const prevBtn = document.getElementById('prev');
    const nextBtn = document.getElementById('next');
    const indicatorsContainer = document.getElementById('indicators');

    let currentIndex = 0;
    const totalSlides = slides.length;

    // Create indicators
    function createIndicators() {
        if (!indicatorsContainer) return;

        for (let i = 0; i < totalSlides; i++) {
            const btn = document.createElement('button');
            btn.addEventListener('click', () => goToSlide(i));
            indicatorsContainer.appendChild(btn);
        }
        updateIndicators();
    }

    // Update indicators
    function updateIndicators() {
        if (!indicatorsContainer) return;

        const buttons = indicatorsContainer.querySelectorAll('button');
        buttons.forEach((btn, index) => {
            btn.classList.toggle('active', index === currentIndex);
        });
    }

    // Go to specific slide
    function goToSlide(index) {
        if (index < 0) {
            currentIndex = totalSlides - 1;
        } else if (index >= totalSlides) {
            currentIndex = 0;
        } else {
            currentIndex = index;
        }

        const offset = -currentIndex * 100;
        track.style.transform = `translateX(${offset}%)`;
        updateIndicators();
    }

    // Next slide
    function nextSlide() {
        goToSlide(currentIndex + 1);
    }

    // Previous slide
    function prevSlide() {
        goToSlide(currentIndex - 1);
    }

    // Event listeners
    if (prevBtn) {
        prevBtn.addEventListener('click', prevSlide);
    }

    if (nextBtn) {
        nextBtn.addEventListener('click', nextSlide);
    }

    // Initialize
    createIndicators();

    // Auto play (optional)
    let autoPlayInterval;

    function startAutoPlay() {
        autoPlayInterval = setInterval(nextSlide, 5000);
    }

    function stopAutoPlay() {
        clearInterval(autoPlayInterval);
    }

    // Start auto play
    startAutoPlay();

    // Pause on hover
    carousel.addEventListener('mouseenter', stopAutoPlay);
    carousel.addEventListener('mouseleave', startAutoPlay);

    // Touch/swipe support for mobile
    let touchStartX = 0;
    let touchEndX = 0;

    carousel.addEventListener('touchstart', (e) => {
        touchStartX = e.changedTouches[0].screenX;
    });

    carousel.addEventListener('touchend', (e) => {
        touchEndX = e.changedTouches[0].screenX;
        handleSwipe();
    });

    function handleSwipe() {
        if (touchEndX < touchStartX - 50) {
            nextSlide();
        }
        if (touchEndX > touchStartX + 50) {
            prevSlide();
        }
    }

    // Keyboard navigation
    document.addEventListener('keydown', (e) => {
        if (e.key === 'ArrowLeft') {
            prevSlide();
        } else if (e.key === 'ArrowRight') {
            nextSlide();
        }
    });

    console.log('✅ Car carousel initialized with', totalSlides, 'slides');
});

// ========== SMOOTH SCROLL ==========
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    });
});

// ========== LAZY LOAD IMAGES ==========
if ('IntersectionObserver' in window) {
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                if (img.dataset.src) {
                    img.src = img.dataset.src;
                    img.removeAttribute('data-src');
                }
                observer.unobserve(img);
            }
        });
    });

    document.querySelectorAll('img[data-src]').forEach(img => {
        imageObserver.observe(img);
    });
}

// ========== VIDEO AUTO PLAY ON SCROLL ==========
const videos = document.querySelectorAll('video');
if (videos.length > 0) {
    const videoObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.play();
            } else {
                entry.target.pause();
            }
        });
    }, { threshold: 0.5 });

    videos.forEach(video => {
        videoObserver.observe(video);
    });
}

console.log('✅ Index.js loaded successfully!');