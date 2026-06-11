// edit.js

document.addEventListener("DOMContentLoaded", function () {
    const mediaSelect = document.getElementById('mediaSelect');
    const optionsGroup = document.getElementById('optionsCheckboxGroup');
    const locationGroup = document.getElementById('locationGroup');
    const toggleReviewsBtn = document.getElementById('toggleReviewsBtn');
    const title = document.getElementById('bookTitleHidden')?.value;

    // 1. Media format visibility toggle logic
    function toggleOptionsVisibility() {
        if (!mediaSelect) return;

        const selectedText = mediaSelect.options[mediaSelect.selectedIndex].text.toLowerCase();
        const isPrinted = selectedText.includes('printed') || selectedText.includes('print');

        if (isPrinted) {
            if (optionsGroup) optionsGroup.classList.remove('d-none');
            if (locationGroup) locationGroup.classList.remove('d-none');
        } else {
            if (optionsGroup) optionsGroup.classList.add('d-none');
            if (locationGroup) locationGroup.classList.add('d-none');

            const bookmarkCheckbox = document.getElementById('Bookmark');
            if (bookmarkCheckbox) bookmarkCheckbox.checked = false;

            const locationInput = document.getElementById('bookLocation');
            if (locationInput) locationInput.value = '';
        }
    }

    // Initialize media visibility logic if element exists
    if (mediaSelect) {
        toggleOptionsVisibility();
        mediaSelect.addEventListener('change', toggleOptionsVisibility);
    }

    // 2. Load reviews if title exists
    if (title) {
        loadBookReviews(title);
    }

    // 3. Toggle reviews visibility handler
    toggleReviewsBtn?.addEventListener('click', function () {
        const reviewsList = document.getElementById('reviewsList');
        if (!reviewsList) return;

        const isExpanded = reviewsList.classList.toggle('expanded');
        const reviewCount = reviewsList.children.length;

        if (isExpanded) {
            this.innerHTML = 'Show fewer reviews';
        } else {
            this.innerHTML = `Show all ${reviewCount} review${reviewCount > 1 ? 's' : ''}`;
        }
    });
});

// Fetching reviews from API
function loadBookReviews(title) {
    const url = `/api/reviews?title=${encodeURIComponent(title)}`;

    fetch(url)
        .then(response => response.json())
        .then(data => {
            const reviewsList = document.getElementById('reviewsList');
            const placeholderText = document.getElementById('placeholderText');
            const toggleReviewsBtn = document.getElementById('toggleReviewsBtn');

            if (!reviewsList || !placeholderText || !toggleReviewsBtn) return;

            reviewsList.innerHTML = '';
            placeholderText.classList.add('d-none');
            toggleReviewsBtn.classList.add('d-none');

            if (data && data.length > 0) {
                reviewsList.classList.remove('d-none');

                data.forEach(review => {
                    const listItem = document.createElement('li');
                    listItem.className = 'mb-3';
                    listItem.innerHTML = `
                        <div class="card p-3 shadow-sm border-0 rounded">
                            <div class="d-flex justify-content-between align-items-start mb-2">
                                <div>
                                    <strong>${escapeHtml(review.userName)}</strong>
                                    <span class="badge rounded-pill text-bg-primary ms-2">
                                        ${review.rating} <i class="bi bi-star-fill"></i>
                                    </span>
                                </div>
                                <small class="text-muted">${new Date(review.createdAt).toLocaleString()}</small>
                            </div>
                            <p class="mb-0">${escapeHtml(review.comment)}</p>
                        </div>
                    `;
                    reviewsList.appendChild(listItem);
                });

                const reviewCount = data.length;
                toggleReviewsBtn.classList.remove('d-none');
                toggleReviewsBtn.innerHTML = `Show all ${reviewCount} review${reviewCount > 1 ? 's' : ''}`;
            } else {
                placeholderText.classList.remove('d-none');
            }
        })
        .catch(error => console.error('Error loading reviews:', error));
}

// Security Helper: Prevents XSS injections from community comments
function escapeHtml(str) {
    if (!str) return '';
    return str.replace(/[&<>"']/g, function (match) {
        const escapeMap = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#39;'
        };
        return escapeMap[match];
    });
}