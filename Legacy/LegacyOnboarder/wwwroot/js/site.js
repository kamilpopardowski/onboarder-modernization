// One giant jQuery file, handling everything, forever.

$(document).ready(function () {
    
    function initSelect2Lookups() {
        if (!$.fn.select2) return; 

        $('.select2-lookup').each(function () {
            var $s = $(this);
            
            if ($s.hasClass('select2-hidden-accessible')) {
                return;
            }

            var placeholder = $s.attr('title') || '';
            
            var $modalParent = $s.closest('.modal');

            $s.select2({
                width: '100%',
                placeholder: placeholder,
                allowClear: true,
                dropdownParent: $modalParent.length ? $modalParent : $(document.body)
            });
        });
    }
    
    $(function () {
        initSelect2Lookups();
    });

    function setSelectValue($select, value) {
        // If bootstrap-select is available and this select is a selectpicker
        if ($.fn.selectpicker && $select.hasClass('selectpicker')) {
            if (value === null || value === undefined || value === '') {
                $select.selectpicker('val', '');
            } else {
                $select.selectpicker('val', String(value));
            }
            // ensure UI sync
            $select.selectpicker('refresh');
        } else {
            // plain select fallback
            if (value === null || value === undefined || value === '') {
                $select.val('');
            } else {
                $select.val(String(value));
            }
            $select.trigger('change');
        }
    }
    
    function getEmployeeModal() {
        var el = document.getElementById('modal-new-employee');
        if (!el) return null;
        var modal = bootstrap.Modal.getInstance(el);
        if (!modal) {
            modal = new bootstrap.Modal(el);
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

        if ($.fn.selectpicker) {
            $('#department').selectpicker('val', '');
            $('#title').selectpicker('val', '');
            $('#employee-type').selectpicker('val', '');
            $('#supervisor').selectpicker('val', '');
        } else {
            $('#department').val('');
            $('#title').val('');
            $('#employee-type').val('');
            $('#supervisor').val('');
        }

        $('#customTitleInput').val('').hide();

        $('#start-date').val('');
        $('#termination-date').val('');
        $('#rehire').prop('checked', false);
        
        updateDateVisibility();

        $('#employeeModalTitle').text('New Employee');
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
        var modal = getEmployeeModal();
        if (modal) modal.show();
    });

    $('#requests-table').on('click', '.btn-edit-request', function () {

        clearEmployeeForm();

        var $row = $(this).closest('tr');

        var requestId    = $row.data('request-id');
        var firstName    = $row.data('first-name');
        var lastName     = $row.data('last-name');
        var departmentId = $row.data('department-id');
        var titleId      = $row.data('title-id');
        var customTitle  = $row.data('custom-title');
        var employeeType = $row.data('employee-type');
        var supervisorId = $row.data('supervisor');
        var startDate    = $row.data('start-date');
        var termDate     = $row.data('termination-date');

        // IMPORTANT: read raw attributes for booleans
        var isOffRaw = $row.attr('data-is-offboarding');  // "True" / "False" / "true" / "false"
        var rehireRaw = $row.attr('data-rehire');         // same story

        var isOff   = String(isOffRaw || '').toLowerCase() === 'true';
        var isRehire = String(rehireRaw || '').toLowerCase() === 'true';

        $('#employeeModalTitle').text('Edit Employee Request #' + requestId);
        $('#request-id').val(requestId);
        $('#first-name').val(firstName);
        $('#last-name').val(lastName);

        // set request type select correctly
        $('#request-type').val(isOff ? 'true' : 'false');
        updateDateVisibility();

        // Department
        setSelectValue($('#department'), departmentId);

        // Employee Type
        setSelectValue($('#employee-type'), employeeType);

        // Supervisor
        setSelectValue($('#supervisor'), supervisorId);

        // Title + custom title
        if (titleId && $("#title option[value='" + titleId + "']").length > 0) {
            setSelectValue($('#title'), titleId);
            $('#customTitleInput').hide().val('');
        } else if (customTitle && customTitle.length > 0) {
            setSelectValue($('#title'), -1);
            $('#customTitleInput').show().val(customTitle);
        } else {
            setSelectValue($('#title'), '');
            $('#customTitleInput').hide().val('');
        }

        // Dates + rehire
        $('#start-date').val(startDate || '');
        $('#termination-date').val(termDate || '');
        $('#rehire').prop('checked', isRehire);

        var modal = getEmployeeModal();
        if (modal) modal.show();
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

    // initial state
    updateDateVisibility();
    $('#request-type').trigger('change');
});
