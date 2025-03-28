// Footer slide up/down effect
$(document).ready(function () {
    let footerVisible = true;
    const footer = $('.footer-gradient');
    const footerHeight = footer.outerHeight();

    // Ẩn footer ban đầu
    footer.css({
        'transform': 'translateY(' + footerHeight + 'px)',
        'transition': 'transform 0.3s ease-in-out'
    });

    // Hiển thị footer khi cuộn đến cuối trang
    $(window).scroll(function () {
        if ($(window).scrollTop() + $(window).height() > $(document).height() - 100) {
            if (!footerVisible) {
                footer.css('transform', 'translateY(0)');
                footerVisible = true;
            }
        } else {
            if (footerVisible) {
                footer.css('transform', 'translateY(' + footerHeight + 'px)');
                footerVisible = false;
            }
        }
    });

    // Hiển thị footer khi hover vào vùng dưới cùng của trang
    $(document).on('mousemove', function (e) {
        if (e.clientY > $(window).height() - 50) {
            if (!footerVisible) {
                footer.css('transform', 'translateY(0)');
                footerVisible = true;
            }
        } else {
            if (footerVisible && $(window).scrollTop() + $(window).height() < $(document).height() - 100) {
                footer.css('transform', 'translateY(' + footerHeight + 'px)');
                footerVisible = false;
            }
        }
    });
});