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

    // show/hide start vs termination date - on/off boarding
    function updateDateVisibility() {
        var isOff = $('#request-type').val() === 'true';

        // show start date, hide termination date
        $('.start-date-row').toggle(!isOff);
        $('.termination-date-row').toggle(isOff);
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

        // default: onboarding
        //$('#request-type').val('false');
        $('#employeeModalTitle').text('New Employee');

        updateDateVisibility();
    }

    var tableSelector = '#requests-table';

    $('#request-type').on('change', function () {
        updateDateVisibility();

        var selected = $(this).val(); // "false" or "true"
        var wantOffboarding = (selected === 'true');

        $(tableSelector + ' tbody tr').each(function () {
            var isOff = $(this).attr('data-is-offboarding'); // "True" / "False"
            if (isOff) isOff = isOff.toString().toLowerCase();

            var rowIsOff = (isOff === 'true');

            if (wantOffboarding) {
                // show only offboarding
                $(this).toggle(rowIsOff);
            } else {
                // show only onboarding
                $(this).toggle(!rowIsOff);
            }
        });
    });

    $('#request-type').trigger('change');

    // "New Employee" button
    $('#btn-new-employee').on('click', function () {
        clearEmployeeForm();
        getEmployeeModal().show();
    });

    // Edit existing request
    $('#requests-table').on('click', '.btn-edit-request', function () {
        var $row = $(this).closest('tr');

        $('#request-id').val($row.data('request-id'));
        $('#first-name').val($row.data('first-name'));
        $('#last-name').val($row.data('last-name'));
        $('#department').val($row.data('department'));
        $('#title').val($row.data('title'));
        $('#employee-type').val($row.data('employee-type'));
        $('#supervisor').val($row.data('supervisor'));

        var startDate        = $row.data('start-date');
        var terminationDate  = $row.data('termination-date');
        var customTitle      = $row.data('custom-title');
        var rehireRaw        = $row.data('rehire');
        var isOffboardingRaw = $row.data('is-offboarding');

        $('#start-date').val(startDate || '');
        $('#termination-date').val(terminationDate || '');

        // Rehire checkbox
        var rehireBool =
            rehireRaw === true ||
            String(rehireRaw).toLowerCase() === 'true' ||
            rehireRaw === 1 ||
            rehireRaw === '1';

        $('#rehire').prop('checked', rehireBool);

        // Request type select
        var isOffboardingBool =
            isOffboardingRaw === true ||
            String(isOffboardingRaw).toLowerCase() === 'true' ||
            isOffboardingRaw === 1 ||
            isOffboardingRaw === '1';

        $('#request-type').val(isOffboardingBool ? 'true' : 'false');

        if (customTitle && customTitle.length > 0) {
            $('#customTitleInput').val(customTitle).show();
            $('#title').val('-1'); // "Other"
        } else {
            $('#customTitleInput').val('').hide();
        }

        $('#employeeModalTitle')
            .text('Edit Employee Request #' + $row.data('request-id'));

        updateDateVisibility(); // make date rows match request type
        getEmployeeModal().show();
    });

    // Custom title
    $('#title').on('change', function () {
        var value = $(this).val();
        if (value === '-1') {
            $('#customTitleInput').show();
        } else {
            $('#customTitleInput').hide().val('');
        }
    });

    // Debug log if both dates set
    $('#start-date, #termination-date').on('change', function () {
        var start = $('#start-date').val();
        var end = $('#termination-date').val();

        if (start && end) {
            console.warn('Both start and termination dates are set.');
        }
    });

    // Log which button submitted the form
    $('#employeeForm').on('submit', function () {
        console.log('Submitting employee form with submit=' +
            $(document.activeElement).attr('value'));
    });

    // Initial state if modal gets opened somehow on page load
    updateDateVisibility();
});
