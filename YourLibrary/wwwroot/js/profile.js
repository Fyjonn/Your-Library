function toggleEditMode(isEdit) {
    var viewMode = document.getElementById("profile-view-mode");
    var editMode = document.getElementById("profile-edit-mode");
    var widgetsRow = document.getElementById("dashboard-widgets-row");

    if (isEdit) {
        viewMode.style.display = "none";
        editMode.style.display = "block";
        if (widgetsRow) {
            widgetsRow.style.setProperty("display", "none", "important");
        }
    } else {
        viewMode.style.display = "block";
        editMode.style.display = "none";
        if (widgetsRow) {
            widgetsRow.style.display = "flex";
        }
    }
}

document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll('.dashboard-section-card').forEach(card => {
        card.addEventListener('click', function () {
            const sectionUrl = this.getAttribute('data-section-url');
            if (sectionUrl) {
                window.location.href = sectionUrl;
            }
        });
    });

    const profileContainer = document.querySelector('.library-profile-container');

    const stayInEditMode = (profileContainer?.getAttribute('data-stay-in-edit') || 'false').toLowerCase();
    if (stayInEditMode === 'true') {
        toggleEditMode(true);
    }

    const hasProfileError = (profileContainer?.getAttribute('data-has-error') || 'false').toLowerCase() === 'true';
    if (hasProfileError) {
        const cancelButton = document.getElementById("cancelEditButton");
        if (cancelButton) {
            cancelButton.disabled = true;
        }
    }

    const avatarInput = document.getElementById("avatarUpload");
    if (avatarInput) {
        avatarInput.addEventListener("change", function () {
            const file = this.files[0];
            if (!file) return;

            const allowedTypes = ["image/jpeg", "image/png"];
            const maxSize = 5 * 1024 * 1024;

            if (!allowedTypes.includes(file.type)) {
                alert("Only JPG and PNG images are allowed");
                this.value = "";
                return;
            }

            if (file.size > maxSize) {
                alert("File size cannot exceed 5MB");
                this.value = "";
                return;
            }
        });
    }

    const loginInput = document.getElementById("loginInput");
    const emailInput = document.getElementById("emailInput");
    const loginMessage = document.getElementById("loginExistsMessage");
    const emailMessage = document.getElementById("emailExistsMessage");
    const confirmButton = document.getElementById("confirmProfileButton");

    let loginTaken = false;
    let emailTaken = false;

    function updateConfirmButton() {
        if (!confirmButton) return;
        confirmButton.disabled = loginTaken || emailTaken;
    }

    if (emailInput) {
        emailInput.addEventListener("focus", function () {
            emailMessage.style.display = "none";
            emailTaken = false;
            updateConfirmButton();
        });

        emailInput.addEventListener("blur", async function () {
            const email = this.value;
            if (!email) return;

            const response = await fetch(`/Validation/CheckProfileEmail?email=${encodeURIComponent(email)}`);
            const exists = await response.json();

            emailTaken = exists;
            if (exists) {
                emailMessage.style.display = "block";
            }
            updateConfirmButton();
        });
    }

    if (loginInput) {
        loginInput.addEventListener("focus", function () {
            loginMessage.style.display = "none";
            loginTaken = false;
            updateConfirmButton();
        });

        loginInput.addEventListener("blur", async function () {
            const login = this.value;
            if (!login) return;

            const response = await fetch(`/Validation/CheckProfileLogin?login=${encodeURIComponent(login)}`);
            const exists = await response.json();

            loginTaken = exists;
            if (exists) {
                loginMessage.style.display = "block";
            }
            updateConfirmButton();
        });
    }

    // NEW adding own photo 
    const fileInput = document.getElementById("avatarUpload");
    const fileNameLabel = document.getElementById("selectedFileName");

    if (fileInput) {
        fileInput.addEventListener("change", function () {
            if (this.files.length > 0) {
                fileNameLabel.textContent = this.files[0].name;
            }
            else {
                fileNameLabel.textContent = "No file selected";
            }
        });
    }
});