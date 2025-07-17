
function submitEmployeeForm() {
    const form = document.getElementById('createEmployeeForm');
    const formData = new FormData(form);

    fetch('@Url.Action("CreateEmployeeModal", "Admin")', {
        method: 'POST',
        body: formData
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            const modal = bootstrap.Modal.getInstance(document.getElementById('employeeModal'));
            modal.hide();
            alert(data.message);
            location.reload();
        } else {
            alert('Error: ' + data.message);
            if (data.errors) {
                console.log('Validation errors:', data.errors);
            }
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('An error occurred while creating the employee.');
    });
}