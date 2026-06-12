let debounceTimer;

// OBSŁUGA WYSZUKIWANIA I WYBORU Z API
document.getElementById('apiSearchQuery').addEventListener('input', function () {
    const originalQuery = this.value.trim();
    const resultsDiv = document.getElementById('apiResults');
    if (!originalQuery) {
        resultsDiv.innerHTML = '';
        resultsDiv.classList.add('d-none');
        return;
    }
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(() => {
        resultsDiv.innerHTML = '<div class="list-group-item text-muted bg-transparent border-0 italic-loading">🔍 Searching...</div>';
        resultsDiv.classList.remove('d-none');
        fetch(`/api/books/search?query=${encodeURIComponent(originalQuery)}`)
            .then(response => response.json())
            .then(data => {
                resultsDiv.innerHTML = '';
                if (!data.items || data.items.length === 0) {
                    resultsDiv.innerHTML = '<div class="list-group-item text-danger bg-transparent border-0">No book found.</div>';
                    return;
                }
                data.items.forEach(item => {
                    const title = item.volumeInfo.title || "No title";
                    const authors = item.volumeInfo.authors ? item.volumeInfo.authors.join(', ') : "Unknown author";
                    const description = item.volumeInfo.description || "";
                    const genre = item.volumeInfo.categories ? item.volumeInfo.categories.join(', ') : "";
                    const imageUrl = item.volumeInfo.imageLinks ? item.volumeInfo.imageLinks.thumbnail : "https://via.placeholder.com/40x60?text=No+Cover";
                    const ageRating = item.volumeInfo.maturityRating || "NOT_MATURE";

                    const button = document.createElement('button');
                    button.type = 'button';
                    button.className = 'list-group-item list-group-item-action api-result-item d-flex align-items-center gap-3';
                    button.innerHTML = `
                        <img src="${imageUrl}" class="rounded shadow-sm" style="width: 40px; height: 55px; object-fit: cover;" />
                        <div>
                            <strong class="d-block text-dark">${title}</strong>
                            <small class="text-muted">${authors}</small>
                        </div>
                    `;

                    button.addEventListener('click', function () {
                        document.getElementById('bookTitle').value = title;
                        document.getElementById('bookAuthor').value = authors;
                        document.getElementById('bookGenre').value = genre;
                        document.getElementById('bookAgeRating').value = ageRating;
                        document.getElementById('bookDescription').value = description;

                        document.getElementById('bookImageUrl').value = imageUrl;
                        document.getElementById('bookCoverPreview').src = imageUrl;
                        document.getElementById('coverPreviewContainer').classList.remove('d-none');

                        resultsDiv.innerHTML = '';
                        resultsDiv.classList.add('d-none');

                        toggleReviewFormVisibility();
                        loadDatabaseReviews(title, authors);
                    });
                    resultsDiv.appendChild(button);
                });
            })
            .catch(() => {
                resultsDiv.innerHTML = '<div class="list-group-item text-danger bg-transparent border-0">Error fetching data.</div>';
            });
    }, 300);
});

document.addEventListener('click', function (e) {
    const resultsDiv = document.getElementById('apiResults');
    if (e.target.id !== 'apiSearchQuery' && resultsDiv) {
        resultsDiv.classList.add('d-none');
    }
});

// TRYB RĘCZNEGO DODAWANIA KSIĄŻEK
const switchManualMode = document.getElementById('switchManualMode');
const apiSearchSection = document.getElementById('apiSearchSection');
const manualCoverGroup = document.getElementById('manualCoverGroup');
const manualCoverFile = document.getElementById('manualCoverFile');

const fieldsToToggle = [
    document.getElementById('bookTitle'),
    document.getElementById('bookAuthor'),
    document.getElementById('bookGenre'),
    document.getElementById('bookAgeRating'),
    document.getElementById('bookDescription')
];

switchManualMode.addEventListener('change', function () {
    const isManual = this.checked;

    if (isManual) {
        apiSearchSection.classList.add('d-none');
        manualCoverGroup.classList.remove('d-none');
        fieldsToToggle.forEach(f => { if (f) f.removeAttribute('readonly'); });
    } else {
        apiSearchSection.classList.remove('d-none');
        manualCoverGroup.classList.add('d-none');
        fieldsToToggle.forEach(f => { if (f) f.setAttribute('readonly', 'readonly'); });
    }
    toggleReviewFormVisibility();
});

// Podgląd obrazka ładowanego ręcznie
manualCoverFile.addEventListener('change', function () {
    const file = this.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = function (e) {
            document.getElementById('bookCoverPreview').src = e.target.result;
            document.getElementById('coverPreviewContainer').classList.remove('d-none');
        }
        reader.readAsDataURL(file);
    }
});

// rezcne wpisy
document.getElementById('bookTitle').addEventListener('input', function () {
    const t = this.value.trim();
    const a = document.getElementById('bookAuthor').value.trim();
    toggleReviewFormVisibility();
    if (t) loadDatabaseReviews(t, a);
});
document.getElementById('bookAuthor').addEventListener('input', function () {
    toggleReviewFormVisibility();
});

const mediaSelect = document.getElementById('mediaSelect');
const optionsGroup = document.getElementById('optionsCheckboxGroup');
const locationGroup = document.getElementById('locationGroup');

function toggleOptionsVisibility() {
    if (!mediaSelect || mediaSelect.selectedIndex === -1) return;
    const selectedText = mediaSelect.options[mediaSelect.selectedIndex].text.toLowerCase();

    if (selectedText.includes('ebook') || selectedText.includes('audiobook')) {
        optionsGroup.classList.add('d-none');
        locationGroup.classList.add('d-none');

        document.getElementById('Bookmark').checked = false;
        document.getElementById('IsOwned').checked = false;
        if (document.getElementById('bookLocation')) document.getElementById('bookLocation').value = '';
    } else {
        optionsGroup.classList.remove('d-none');
        locationGroup.classList.remove('d-none');
    }
}
mediaSelect.addEventListener('change', toggleOptionsVisibility);

// LOGIKA AKTYWACJI I WALIDACJI OPINII
const readStatusSelect = document.getElementById('readStatusSelect');
const reviewInputGroup = document.getElementById('reviewInputGroup');
const reviewCommentInput = document.getElementById('reviewCommentInput');
const reviewRatingSelect = document.getElementById('reviewRatingSelect');
const reviewBlockedAlert = document.getElementById('reviewBlockedAlert');
const reviewFieldsContainer = document.getElementById('reviewFieldsContainer');

function toggleReviewFormVisibility() {
    if (!readStatusSelect || readStatusSelect.selectedIndex === -1) return;
    const selectedText = readStatusSelect.options[readStatusSelect.selectedIndex].text.toLowerCase();
    const isReadOrDnf = (selectedText.includes('read') && !selectedText.includes('to') && !selectedText.includes('ing')) || selectedText.includes('dnf');

    if (isReadOrDnf) {
        reviewInputGroup.classList.remove('d-none');
        const tFilled = document.getElementById('bookTitle').value.trim();
        const aFilled = document.getElementById('bookAuthor').value.trim();

        if (!tFilled || !aFilled) {
            reviewBlockedAlert.classList.remove('d-none');
            reviewFieldsContainer.classList.add('disabled-element-group');
            if (reviewCommentInput) reviewCommentInput.disabled = true;
            if (reviewRatingSelect) reviewRatingSelect.disabled = true;
        } else {
            reviewBlockedAlert.classList.add('d-none');
            reviewFieldsContainer.classList.remove('disabled-element-group');
            if (reviewCommentInput) reviewCommentInput.disabled = false;
            if (reviewRatingSelect) reviewRatingSelect.disabled = false;
        }
    } else {
        reviewInputGroup.classList.add('d-none');
        if (reviewCommentInput) reviewCommentInput.value = '';
        if (reviewRatingSelect) reviewRatingSelect.value = '0';
        updateVisualStars(0);
    }
}
readStatusSelect.addEventListener('change', toggleReviewFormVisibility);

const visualStars = document.querySelectorAll('.visual-star');
if (reviewRatingSelect) {
    reviewRatingSelect.addEventListener('change', function () {
        updateVisualStars(parseFloat(this.value));
    });
}

function updateVisualStars(rating) {
    visualStars.forEach(star => {
        const starValue = parseInt(star.getAttribute('data-value'));
        if (rating >= starValue) {
            star.className = 'bi bi-star-fill visual-star text-warning';
        } else if (rating > (starValue - 1) && rating < starValue) {
            star.className = 'bi bi-star-half visual-star text-warning';
        } else {
            star.className = 'bi bi-star visual-star';
        }
    });
}

// POBIERANIE REVIEWS
function loadDatabaseReviews(bookTitle, bookAuthor) {
    if (!bookTitle) return;
    fetch(`/api/reviews?title=${encodeURIComponent(bookTitle)}&author=${encodeURIComponent(bookAuthor || '')}`)
        .then(response => {
            if (!response.ok) throw new Error("Database error");
            return response.json();
        })
        .then(reviews => {
            const container = document.getElementById('reviewsContainer');
            const placeholder = document.getElementById('reviewsPlaceholder');
            const list = document.getElementById('reviewsList');
            const toggleBtn = document.getElementById('btnToggleReviews');

            list.innerHTML = '';

            if (!reviews || reviews.length === 0) {
                container.classList.add('d-none');
                placeholder.classList.remove('d-none');
                document.getElementById('placeholderText').innerText = "No one has reviewed this book yet. Be the first!";
                return;
            }

            placeholder.classList.add('d-none');
            container.classList.remove('d-none');

            reviews.forEach((rev, index) => {
                const item = document.createElement('div');
                item.className = `p-3 mb-3 rounded border review-item-box shadow-sm ${index >= 3 ? 'review-hidden-item' : ''}`;

                let starsHtml = '';
                for (let s = 1; s <= 10; s++) {
                    if (rev.rating >= s) starsHtml += '<i class="bi bi-star-fill text-warning me-1"></i>';
                    else if (rev.rating > (s - 1) && rev.rating < s) starsHtml += '<i class="bi bi-star-half text-warning me-1"></i>';
                    else starsHtml += '<i class="bi bi-star text-muted me-1"></i>';
                }

                item.innerHTML = `
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <span class="fw-bold text-dark"><i class="bi bi-person-circle me-1"></i> ${rev.user}</span>
                        <small class="text-muted"><i class="bi bi-calendar3 me-1"></i> ${rev.date}</small>
                    </div>
                    <div class="mb-2 fs-6">${starsHtml} <span class="badge bg-secondary ms-1">${rev.rating}</span></div>
                    <p class="mb-0 text-secondary italic-comment" style="font-size: 0.95rem;">"${rev.comment}"</p>
                `;
                list.appendChild(item);
            });

            if (reviews.length > 3) {
                toggleBtn.classList.remove('d-none');
                toggleBtn.innerText = "Show all reviews";
                toggleBtn.replaceWith(toggleBtn.cloneNode(true));
                const newToggleBtn = document.getElementById('btnToggleReviews');

                newToggleBtn.addEventListener('click', function () {
                    const hiddenItems = list.querySelectorAll('.review-hidden-item');
                    if (hiddenItems.length > 0) {
                        hiddenItems.forEach(item => item.classList.remove('review-hidden-item'));
                        this.innerText = "Hide extra reviews";
                    } else {
                        const allItems = list.querySelectorAll('.review-item-box');
                        allItems.forEach((item, idx) => {
                            if (idx >= 3) item.classList.add('review-hidden-item');
                        });
                        this.innerText = "Show all reviews";
                    }
                });
            } else {
                toggleBtn.classList.add('d-none');
            }
        })
        .catch(() => {
            document.getElementById('placeholderText').innerText = "Select or add a book to load user reviews and thoughts.";
        });
}

// Inicjalizacja formularza na start
if (mediaSelect && mediaSelect.value) toggleOptionsVisibility();
if (readStatusSelect && readStatusSelect.value) {
    toggleReviewFormVisibility();
    if (reviewRatingSelect) {
        updateVisualStars(parseFloat(reviewRatingSelect.value || 0));
    }
}