
function initRowClickListeners() {
    document.querySelectorAll('.book-clickable-row').forEach(row => {
        row.addEventListener('click', function () {
            const url = this.getAttribute('data-url');
            if (url) {
                window.location.href = url;
            }
        });
    });
}

function filterBooks() {
    const searchInput = document.getElementById('shelfSearchInput');
    const radioTitle = document.getElementById('searchByTitle');

    if (!searchInput || !radioTitle) return;

    const query = searchInput.value.trim().toLowerCase();
    const searchBy = radioTitle.checked ? 'title' : 'author';

    const activePane = document.querySelector('.shelf-pane.d-block');
    if (!activePane) return;

    const books = activePane.querySelectorAll('.book-card-item');
    let visibleCount = 0;

    books.forEach(book => {
        const attrValue = book.getAttribute(`data-${searchBy}`) || '';

        if (attrValue.includes(query)) {
            book.classList.remove('d-none');
            visibleCount++;
        } else {
            book.classList.add('d-none');
        }
    });

    let noResultsMsg = activePane.querySelector('.no-search-results');
    const defaultEmptyMsg = activePane.querySelector('.no-books-msg');

    if (visibleCount === 0 && query !== '') {
        if (!noResultsMsg && !defaultEmptyMsg) {
            noResultsMsg = document.createElement('p');
            noResultsMsg.className = 'text-center w-100 py-4 no-search-results custom-error-msg';
            noResultsMsg.textContent = 'No books found that match your filters.';
            activePane.querySelector('.container-books').appendChild(noResultsMsg);
        } else if (noResultsMsg) {
            noResultsMsg.classList.remove('d-none');
        }
    } else {
        if (noResultsMsg) {
            noResultsMsg.classList.add('d-none');
            noResultsMsg.style.setProperty('display', 'none', 'important');
        }
    }
}

document.addEventListener("DOMContentLoaded", function () {
    initRowClickListeners();

    document.querySelectorAll('.dropdown-item').forEach(item => {
        item.addEventListener('click', function () {
            const shelfName = this.textContent.trim();
            const targetSelector = this.getAttribute('data-shelf-target');

            const shelfTitleEl = document.getElementById('current-shelf-title');
            if (shelfTitleEl) {
                shelfTitleEl.textContent = shelfName;
            }

            document.querySelectorAll('.shelf-pane').forEach(pane => {
                pane.classList.remove('d-block');
                pane.classList.add('d-none');
            });

            const targetPane = document.querySelector(targetSelector);
            if (targetPane) {
                targetPane.classList.remove('d-none');
                targetPane.classList.add('d-block');
            }

            document.querySelectorAll('.dropdown-item').forEach(el => el.classList.remove('active'));
            this.classList.add('active');

            const searchInput = document.getElementById('shelfSearchInput');
            if (searchInput) {
                searchInput.value = '';
                filterBooks();
            }
        });
    });

    const searchInput = document.getElementById('shelfSearchInput');
    if (searchInput) {
        searchInput.addEventListener('input', filterBooks);
    }

    document.querySelectorAll('input[name="searchType"]').forEach(radio => {
        radio.addEventListener('change', filterBooks);
    });
});