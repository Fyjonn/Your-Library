
function filterFriendsList() {
    let input = document.getElementById('inlineFriendSearch').value.toLowerCase().trim();
    let items = document.querySelectorAll('.friend-list-item-row');

    items.forEach(function (item) {
        let username = item.getAttribute('data-username');

        if (username && username.includes(input)) {
            item.style.setProperty('display', 'flex', 'important');
        } else {
            item.style.setProperty('display', 'none', 'important');
        }
    });
}


document.addEventListener("DOMContentLoaded", function () {
    const deleteFriendModal = document.getElementById('deleteFriendModal');

    if (deleteFriendModal) {
        deleteFriendModal.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget;

            const friendName = button.getAttribute('data-friend-name');
            const friendshipId = button.getAttribute('data-friend-id');

            const modalFriendNameText = deleteFriendModal.querySelector('#modal-friend-name');
            const modalFriendshipIdInput = deleteFriendModal.querySelector('#friendshipIdInput');

            if (modalFriendNameText) {
                modalFriendNameText.textContent = friendName;
            }
            if (modalFriendshipIdInput) {
                modalFriendshipIdInput.value = friendshipId;
            }
        });
    }
});