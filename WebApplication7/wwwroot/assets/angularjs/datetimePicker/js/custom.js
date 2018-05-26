$(document).ready(function () {
    $(".datetime-picker").click(function () {
        $(this).datetimepicker({
            timepicker: true,
            format: 'd/m/Y H:i',
            mask: '19/39/9999 29:59'
        });
    });
});