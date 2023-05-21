const ticketStatus = {
    0: { 'title': "بسته شده", 'class': 'dark' },
    1: { 'title': "منتظر پاسخ پشتیبان", 'class': 'light-primary' },
    2: { 'title': "منتظر پاسخ کاربر", 'class': 'light-success' },
    3: { 'title': "درانتظار", 'class': 'light-warning' },
};
const ticketPriority = {
    0: { 'title': "فوری", 'class': 'light-danger' },
    1: { 'title': "معمولی", 'class': 'light-info' },
    2: { 'title': "جهت اطلاع", 'class': 'light' },
};
const colors = [
    KTUtil.getCssVariableValue('--bs-blue'),
    KTUtil.getCssVariableValue('--bs-purple'),
    KTUtil.getCssVariableValue('--bs-pink'),
    KTUtil.getCssVariableValue('--bs-red'),
    KTUtil.getCssVariableValue('--bs-orange'),
    KTUtil.getCssVariableValue('--bs-yellow'),
    KTUtil.getCssVariableValue('--bs-green'),
    KTUtil.getCssVariableValue('--bs-teal'),
    KTUtil.getCssVariableValue('--bs-cyan'),
];