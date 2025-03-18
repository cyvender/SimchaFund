$(() => {
    const contributorModal = new bootstrap.Modal($("#contributor-modal")[0]);

    $(".btn-info").on('click', function () {
        contributorModal.show();
    });

    const depositModal = new bootstrap.Modal($("#deposit-modal")[0]);

    $(".btn-success").on('click', function () {
        let contribId = $(this).data("contribid");
        $("#contributor-deposit-id").val(contribId);
        depositModal.show();
    });

    const editModal = new bootstrap.Modal($("#edit-modal")[0]);

    $(".btn-danger").on('click', function () {
        let contribId = $(this).data("contribid");
        $("#contributor-edit-id").val(contribId);
        editModal.show();
    });
})