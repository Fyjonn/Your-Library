let deleteModal;


function openDeleteModal(userId, userName) {
    document.getElementById("modalUserName").innerText = userName;

    const configEl = document.getElementById("adminConfig");
    const baseDeleteUrl = configEl ? configEl.getAttribute("data-delete-url") : '/Admin/DeleteUser';

    const form = document.getElementById("modalDeleteForm");
    form.action = `${baseDeleteUrl}/${userId}`;

    if (!deleteModal) {
        deleteModal = new bootstrap.Modal(document.getElementById('deleteConfirmModal'));
    }
    deleteModal.show();
}


function toggleReviews(rowElement, userId) {
    const row = document.getElementById(`reviews-row-${userId}`);
    const wrapper = document.getElementById(`reviews-wrapper-${userId}`);
    const contentDiv = document.getElementById(`reviews-content-${userId}`);

    if (row && !row.classList.contains('d-none')) {
        wrapper.classList.remove('open');
        rowElement.classList.remove('row-active');
        setTimeout(() => {
            row.classList.add('d-none');
        }, 300);
        return;
    }

    document.querySelectorAll('.reviews-row').forEach(r => r.classList.add('d-none'));
    document.querySelectorAll('.reviews-wrapper').forEach(w => w.classList.remove('open'));
    document.querySelectorAll('.user-row').forEach(tr => tr.classList.remove('row-active'));

    row.classList.remove('d-none');
    rowElement.classList.add('row-active');

    setTimeout(() => {
        wrapper.classList.add('open');
    }, 10);

    fetch(`/Admin/GetUserReviews/${userId}`)
        .then(response => response.json())
        .then(data => {
            contentDiv.innerHTML = "";

            if (data.length === 0) {
                contentDiv.innerHTML = `
                    <div class="no-reviews-placeholder font-serif">
                        <i class="bi bi-info-circle me-2"></i>This user hasn't added any reviews yet.
                    </div>`;
                return;
            }

            data.forEach(review => {
                const item = document.createElement('div');
                item.className = 'custom-review-item';
                item.innerHTML = `
                    <div class="review-item-header d-flex justify-content-between align-items-start gap-3">
                        <div class="book-info">
                            <span class="book-title-text text-dark fs-5 fw-bold">${review.bookTitle}</span>
                            <span class="book-author-text text-muted fs-6 ms-1 font-serif">by ${review.bookAuthor}</span>
                        </div>
                        <span class="badge rating-badge-custom"><i class="bi bi-star-fill me-1 text-warning"></i>${review.rating}/10</span>
                    </div>
                    <div class="review-item-body review-comment-text font-serif">
                        ${review.comment ? review.comment : '<em class="text-muted">No comment text left.</em>'}
                    </div>
                `;
                contentDiv.appendChild(item);
            });
        })
        .catch(error => {
            console.error("Error fetching reviews:", error);
            contentDiv.innerHTML = `
                <div class="p-3 text-danger font-serif">
                    <i class="bi bi-exclamation-circle me-2"></i>Failed to load reviews.
                </div>`;
        });
}

 
document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById("adminSearchInput");
    const clearBtn = document.getElementById("clearSearch");
    const rows = document.querySelectorAll(".user-row");
    const noResultsRow = document.getElementById("noResultsRow");

    if (!searchInput) return; 

    searchInput.addEventListener("input", function () {
        const value = this.value.toLowerCase().trim();
        let hasResults = false;

        if (value.length > 0) {
            clearBtn.classList.remove("d-none");
        } else {
            clearBtn.classList.add("d-none");
        }

        rows.forEach(row => {
            const searchContent = row.getAttribute("data-searchable");
            const userId = row.getAttribute("data-id");
            const associatedRow = document.getElementById(`reviews-row-${userId}`);
            const associatedWrapper = document.getElementById(`reviews-wrapper-${userId}`);

            if (searchContent && searchContent.includes(value)) {
                row.classList.remove("d-none");
                hasResults = true;
            } else {
                row.classList.add("d-none");
                row.classList.remove("row-active");
                if (associatedRow) associatedRow.classList.add('d-none');
                if (associatedWrapper) associatedWrapper.classList.remove('open');
            }
        });

        if (hasResults) {
            noResultsRow.classList.add("d-none");
        } else {
            noResultsRow.classList.remove("d-none");
        }
    });

    if (clearBtn) {
        clearBtn.addEventListener("click", function () {
            searchInput.value = "";
            searchInput.dispatchEvent(new Event("input"));
            searchInput.focus();
        });
    }
});