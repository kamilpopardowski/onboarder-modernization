// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// One giant jQuery file, handling everything, forever.

$(document).ready(function () {
    function getEmployeeModal() {
        var modalElement = document.getElementById('modal-new-employee');
        var modal = bootstrap.Modal.getInstance(modalElement);
        if (!modal) {
            modal = new bootstrap.Modal(modalElement);
        }
        return modal;
    }

    function clearEmployeeForm() {
        $('#request-id').val('0');
        $('#first-name').val('');
        $('#last-name').val('');
        $('#department').val('');
        $('#title').val('');
        $('#customTitleInput').val('').hide();
        $('#employee-type').val('');
        $('#supervisor').val('');
        $('#start-date').val('');
        $('#termination-date').val('');
        $('#rehire').prop('checked', false);
        $('#employeeModalTitle').text('New Employee');
    }

    $('#btn-new-employee').on('click', function () {
        clearEmployeeForm();
        getEmployeeModal().show();
    });

    $('#requestsTable').on('click', '.btn-edit-request', function () {
        var $row = $(this).closest('tr');

        $('#request-id').val($row.data('request-id'));
        $('#first-name').val($row.data('first-name'));
        $('#last-name').val($row.data('last-name'));
        $('#department').val($row.data('department'));
        $('#title').val($row.data('title'));
        $('#employee-type').val($row.data('employee-type'));
        $('#supervisor').val($row.data('supervisor'));

        var startDate       = $row.data('start-date');
        var terminationDate = $row.data('termination-date');
        var customTitle     = $row.data('custom-title');
        var rehireRaw       = $row.data('rehire');
        var isOffboardingRaw = $row.data('is-offboarding');

        $('#start-date').val(startDate || '');
        $('#termination-date').val(terminationDate || '');

        // --- Rehire checkbox: handle "True"/"False", 1/0, true/false ---
        var rehireBool =
            rehireRaw === true ||
            String(rehireRaw).toLowerCase() === 'true' ||
            rehireRaw === 1 ||
            rehireRaw === '1';

        $('#rehire').prop('checked', rehireBool);

        // --- Request type (onboarding/offboarding) select ---
        var isOffboardingBool =
            isOffboardingRaw === true ||
            String(isOffboardingRaw).toLowerCase() === 'true' ||
            isOffboardingRaw === 1 ||
            isOffboardingRaw === '1';

        // our <select> has option values "false" / "true"
        $('#request-type').val(isOffboardingBool ? 'true' : 'false');

        if (customTitle && customTitle.length > 0) {
            $('#customTitleInput').val(customTitle).show();
            $('#title').val('-1'); // "Other"
        } else {
            $('#customTitleInput').val('').hide();
        }

        $('#employeeModalTitle')
            .text('Edit Employee Request #' + $row.data('request-id'));

        getEmployeeModal().show();
    });


    $('#title').on('change', function () {
        var value = $(this).val();
        if (value === '-1') {
            $('#customTitleInput').show();
        } else {
            $('#customTitleInput').hide().val('');
        }
    });

    $('#start-date, #termination-date').on('change', function () {
        var start = $('#start-date').val();
        var end = $('#termination-date').val();

        if (start && end) {
            console.warn('Both start and termination dates are set.');
        }
    });

    $('#employeeForm').on('submit', function () {
        console.log('Submitting employee form with submit=' +
            $(document.activeElement).attr('value'));
    });

});

