document.addEventListener("DOMContentLoaded", function () {
    const mediaSelect = document.getElementById('mediaSelect');
    const mediaSelectHidden = document.getElementById('mediaSelectHidden');

    const optionsGroup = document.getElementById('optionsCheckboxGroup');
    const locationGroup = document.getElementById('locationGroup');
    const readStatusSelect = document.getElementById('readStatusSelect');
    const reviewRatingGroup = document.getElementById('reviewRatingGroup');

    function toggleOptionsVisibility() {
        let selectedText = "";

        if (mediaSelect) {
            selectedText = mediaSelect.options[mediaSelect.selectedIndex].text.toLowerCase();
        } else if (mediaSelectHidden) {
            const val = mediaSelectHidden.value;
            if (val === "0" || val.toLowerCase() === "printed") {
                selectedText = "printed";
            } else {
                selectedText = "other";
            }
        } else {
            if (optionsGroup) optionsGroup.classList.add('d-none');
            if (locationGroup) locationGroup.classList.add('d-none');
            return;
        }

        const isPrinted = selectedText.includes('printed') || selectedText.includes('print');

        if (isPrinted) {
            if (optionsGroup) optionsGroup.classList.remove('d-none');
            if (locationGroup) locationGroup.classList.remove('d-none');

            const bookmarkWrapper = document.getElementById('bookmarkWrapper');
            if (bookmarkWrapper) bookmarkWrapper.classList.remove('d-none');

            const isOwnedWrapper = document.getElementById('isOwnedWrapper');
            if (isOwnedWrapper) isOwnedWrapper.classList.remove('d-none');
        } else {
            // Czyszczenie i ukrywanie pól przy zmianie z printed na inny
            const bookmarkCheckbox = document.getElementById('Bookmark');
            if (bookmarkCheckbox) bookmarkCheckbox.checked = false;

            const isOwnedCheckbox = document.getElementById('IsOwned');
            if (isOwnedCheckbox) isOwnedCheckbox.checked = false;

            const locationInput = document.getElementById('bookLocation');
            if (locationInput) locationInput.value = '';

            if (optionsGroup) optionsGroup.classList.add('d-none');
            if (locationGroup) locationGroup.classList.add('d-none');
        }
    }

    function toggleReviewVisibility() {
        if (!readStatusSelect) {
            // Przeszli pożyczający zawsze widzą formularz oceniania/notatek
            if (reviewRatingGroup) reviewRatingGroup.classList.remove('d-none');
            return;
        }

        const selectedText = readStatusSelect.options[readStatusSelect.selectedIndex].text.toLowerCase();
        const isValidStatus = selectedText === 'read' || selectedText === 'dnf';

        if (isValidStatus) {
            if (reviewRatingGroup) reviewRatingGroup.classList.remove('d-none');
        } else {
            if (reviewRatingGroup) reviewRatingGroup.classList.add('d-none');

            const ratingSelect = document.getElementById('ratingSelect');
            const reviewComment = document.getElementById('reviewCommentText');
            if (ratingSelect) ratingSelect.value = "0";
            if (reviewComment) reviewComment.value = "";
        }
    }

    toggleOptionsVisibility();
    if (mediaSelect) {
        mediaSelect.addEventListener('change', toggleOptionsVisibility);
    }

    toggleReviewVisibility();
    if (readStatusSelect) {
        readStatusSelect.addEventListener('change', toggleReviewVisibility);
    }
});