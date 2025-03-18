$(() => {
    const simchaModal = new bootstrap.Modal($("#simcha-modal")[0]);

    $(".btn-info").on('click', function () {
        //let contribId = $(this).data("contribid");
        //$("#contributor-id").val(contribId);
        simchaModal.show();
    });
})

//$(() => {
//    const contributorModal = new bootstrap.Modal($("#contributor-modal")[0]);

//    $(".btn-info").on('click', function () {
//        contributorModal.show();
//    });

//    const depositModal = new bootstrap.Modal($("#deposit-modal")[0]);

//    $(".btn-success").on('click', function () {
//        let contribId = $(this).data("contribid");
//        $("#contributor-id").val(contribId);
//        depositModal.show();
//    });
//})