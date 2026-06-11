document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll('.dropdown-item').forEach(item => {
        item.addEventListener('click', function () {
            const shelfName = this.textContent.trim();
            const targetSelector = this.getAttribute('data-shelf-target');

            const indicator = document.getElementById('shelf-indicator');
            if (indicator) {
                indicator.textContent = shelfName;
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

function filterBooks() {
    const searchInput = document.getElementById('shelfSearchInput');
    const radioTitle = document.getElementById('searchByTitle');

    if (!searchInput) return;

    const query = searchInput.value.trim().toLowerCase();
    const searchBy = (radioTitle && radioTitle.checked) ? 'title' : 'author';
    const activePane = document.querySelector('.shelf-pane.d-block');

    if (!activePane) return;

    const books = activePane.querySelectorAll('.book-card-item');

    books.forEach(book => {
        const clickableCell = book.querySelector('[data-title]');
        const attrValue = clickableCell ? (clickableCell.getAttribute(`data-${searchBy}`) || '') : '';

        if (attrValue.includes(query)) {
            book.classList.remove('d-none');
        } else {
            book.classList.add('d-none');
        }
    });
}