
var app = angular.module('InfinityLab');

app.controller('BugTrackerController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;

        $scope.Type = [{ 'id': '1', 'name': 'BUG' },
        { 'id': '2', 'name': 'CHANGE REQUEST' },
        { 'id': '3', 'name': 'CLARIFICATION' },
        { 'id': '4', 'name': 'CONFIGURATION' }];
        $scope.Status = [{ 'id': '1', 'name': 'NEW' },
        { 'id': '2', 'name': 'CLOSED' },
        { 'id': '3', 'name': 'REOPEN' },
        { 'id': '4', 'name': 'DUPLICATE' },
        { 'id': '5', 'name': 'PENDING' }];
        $scope.Priority = [{ 'id': '1', 'name': 'LOW' }, { 'id': '2', 'name': 'HIGH' },
        { 'id': '3', 'name': 'CRITICAL' }];
        //$scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
        //    $scope.Employee = results;
        //});

        $scope.Employee = [{ 'id': '76', 'name': 'DEENADAYALAN J' },
        { 'id': '77', 'name': 'ELAYARAJA S' },
        { 'id': '179', 'name': 'SATHYA' },
        { 'id': '85', 'name': 'RAJENDRAN' },
        { 'id': '82', 'name': 'PRASANNA K' },
        { 'id': '79', 'name': 'LOGESHWARAN S P' },
        { 'id': '185', 'name': 'SHENBAGARAMAN M' },
        { 'id': '84', 'name': 'B.BANUMATHY' },
        { 'id': '195', 'name': 'VIGNESHWARAN V ' },
        { 'id': '197', 'name': 'RENUGA DEVI.S' },
        { 'id': '196', 'name': 'MOHAMED JAINULJAVIDH.J' },
        { 'id': '200', 'name': 'T.A.DHANUSH RAM' },
        { 'id': '201', 'name': 'K.GEETHANJALI' },
        { 'id': '208', 'name': 'DEEPKARAN BAFNA' },
        { 'id': '205', 'name': 'PRABAKARAN' },
        { 'id': '207', 'name': 'GAYATHRI R' },
        { 'id': '210', 'name': 'SATHIYAN B' },
        { 'id': '131', 'name': 'CAVIN KUMAR' },
        { 'id': '132', 'name': 'BALA MURUGAN' },
        { 'id': '133', 'name': 'GANESH' }
        ]

        $scope.ImageTypes = [{ 'id': 'Communication', 'name': 'Communication' }];
        $scope.Company = [{ 'id': '1', 'name': 'PAKSHI K RAMANATHAM CHETTIAR SON' },
        { 'id': '3', 'name': 'SHOBA TEXTILE' },
        { 'id': '4', 'name': 'VARDHAMAN TEXTILE' },
        { 'id': '5', 'name': 'P.R.SUBRAMANIA CHETTY&CO' },
        { 'id': '6', 'name': 'RAJA SILK PALACE' },
        { 'id': '7', 'name': 'MARINA TEXTILE' },
        { 'id': '8', 'name': 'UDHAYA TEXTILE' },
        { 'id': '10', 'name': 'PARAMS TEX' },
        { 'id': '11', 'name': 'S.M SILKS' },
        { 'id': '12', 'name': 'PONRAJAN' },
        { 'id': '13', 'name': 'BAJI TEXTILES' },
        { 'id': '14', 'name': 'VPS' },
        { 'id': '15', 'name': 'THIRUPATHY JAVALI KADAL' },
        { 'id': '16', 'name': 'MAHARAJA TEXTILES - RMD' },
        { 'id': '17', 'name': 'MOBILE EXPRESS' },
        { 'id': '18', 'name': 'NEW VPS' },
        { 'id': '19', 'name': 'KOVAI VIGNESHWARA TEXTILES' },
        { 'id': '20', 'name': 'SOUTH INDIA MARKETING' },
        { 'id': '21', 'name': 'MEENAKSHI TEXTILES' },
        { 'id': '22', 'name': 'ABBINAYA READYMADES' },
        { 'id': '23', 'name': 'GANAPATHI GARMENTS' },
        { 'id': '24', 'name': 'SANTHOSH LIFE STYLE' },
        { 'id': '25', 'name': 'MADRAS SUPER STORE' },
        { 'id': '26', 'name': 'NAVANITHAA READYMADES' },
        { 'id': '27', 'name': 'NEW RAJAN TEXTILES' },
        { 'id': '28', 'name': 'KURINJI READYMADES' },
        { 'id': '29', 'name': 'VELS TEX' },
        { 'id': '30', 'name': 'KUMARAN STORES' },
        { 'id': '31', 'name': 'SKY HIGH' },
        { 'id': '32', 'name': 'YOGESH' },
        { 'id': '33', 'name': 'NACH' },
        { 'id': '34', 'name': 'ABIRAMI' },
        { 'id': '35', 'name': 'ZITY ZTYLE' },
        { 'id': '36', 'name': 'MAHARAJA READYMADS - TNJ' },
        { 'id': '37', 'name': 'MAHARAJA SILKS - TNJ' },
        { 'id': '38', 'name': 'MAHARAJA SEEMATTI - TVR' },
        { 'id': '39', 'name': 'MAHARAJA SEEMATTI - KUM' },
        { 'id': '40', 'name': 'LUCKY SILKS' },
        { 'id': '41', 'name': 'THARAGAI READYMADES' },
        { 'id': '42', 'name': 'SHOP IN' },
        { 'id': '43', 'name': 'SARKAR SILKS' },
        { 'id': '44', 'name': 'B3 FASHIONS' },
        { 'id': '45', 'name': 'SARANYA TEXTILES' },
        { 'id': '46', 'name': 'FAASBRO - MALAR BRAND' },
        { 'id': '47', 'name': 'SIVALAYA TEX' },
        { 'id': '48', 'name': 'MAHARANI' },
        { 'id': '49', 'name': 'HAJI' },
        { 'id': '50', 'name': 'SPYKON' },
        { 'id': '51', 'name': 'SMILE FASHION' },
        { 'id': '52', 'name': 'JAYAKUMARAN' },
        { 'id': '53', 'name': 'ONE2ONE' },
        { 'id': '54', 'name': 'SRINIVASA RAGHURAM' },
        { 'id': '55', 'name': 'SHUBHAKKARI SILKS' },
        { 'id': '56', 'name': 'SKB' },
        { 'id': '57', 'name': 'DISHA CLOTHINGS' },
        { 'id': '58', 'name': 'ARUNA JUST KIDDING' },
        { 'id': '59', 'name': 'NAAZ' },
        { 'id': '60', 'name': 'BOMBAY FASHIONS' },
        { 'id': '61', 'name': 'THARAGAI SILKS' },
        { 'id': '62', 'name': 'THARAGAI MATCHINGS' },
        { 'id': '63', 'name': 'RAMESHKUMAR TEXTILES' },
        { 'id': '64', 'name': 'SARANYA DECORUS' },
        { 'id': '65', 'name': 'BIG BOSS - MANANJI' },
        { 'id': '66', 'name': 'STYLE ONE' },
        { 'id': '67', 'name': 'NAKSATRA' },
        { 'id': '68', 'name': 'NEERAJ - ANDAMAN' },
        { 'id': '69', 'name': 'NAGARANI' },
        { 'id': '70', 'name': 'MUMMY DADDY' },
        { 'id': '71', 'name': 'SURIYA MAHALL - ANDAMAN' },
        { 'id': '72', 'name': 'RAJA READYMADES - ARNI' },
        { 'id': '73', 'name': 'SURUTHI SILKS' },
        { 'id': '74', 'name': 'SRI LAKSHMI STORES' },
        { 'id': '75', 'name': 'RAINBOW' },
        { 'id': '76', 'name': 'SANGAGIRI JAULI KADAL' },
        { 'id': '77', 'name': 'KIYA' },
        { 'id': '78', 'name': 'SOORIYA TEXTILES' },
        { 'id': '79', 'name': 'RAJESH TEXTILES' },
        { 'id': '80', 'name': 'PNR LIFE STORE' },
        { 'id': '81', 'name': 'SLEEK' },
        { 'id': '82', 'name': 'DON MENS LAND' },
        { 'id': '83', 'name': 'LAAWIN TEXTILES' },
        { 'id': '84', 'name': 'MN CHETTIYAR' },
        { 'id': '85', 'name': 'GPS SILKS & READYMADES' },
        { 'id': '86', 'name': 'VKS' },
        { 'id': '87', 'name': 'ARUN FASHION' },
        { 'id': '88', 'name': 'SHANUS BOUTIQUE' },
        { 'id': '89', 'name': 'SEEMATI - MAYAVARAM' },
        { 'id': '90', 'name': 'SARKAR SILK HOUSE' },
        { 'id': '91', 'name': 'INDIA SILK HOUSE' },
        { 'id': '92', 'name': 'VIDARSHA FASHIONS' },
        { 'id': '93', 'name': 'SHANUS' },
        { 'id': '94', 'name': 'LOOK AT ME' },
        { 'id': '95', 'name': 'RMR SILKS' },
        { 'id': '96', 'name': 'SRI RAM SILKS' },
        { 'id': '97', 'name': 'ESWAR SILKS' },
        { 'id': '98', 'name': 'MAHA MENS WEAR' },
        { 'id': '99', 'name': 'NACHIMUTHU' },
        { 'id': '100', 'name': 'LAYA BOUTIQUE' },
        { 'id': '101', 'name': 'MTEENZ' },
        { 'id': '102', 'name': 'THILAGARAJ' },
        { 'id': '103', 'name': 'APN SILKS' },
        { 'id': '104', 'name': 'RAMANA SILKS' },
        { 'id': '105', 'name': 'UDHAYAM SILKS' },
        { 'id': '106', 'name': 'PONAMMAL TEXTILES' },
        { 'id': '107', 'name': 'BEST MENWEAR' },
        { 'id': '108', 'name': 'LMC SILKS' },
        { 'id': '109', 'name': 'SB TEX' },
        { 'id': '110', 'name': 'RAYA SILKS' },
        { 'id': '111', 'name': 'AARUDHRA MENS WEAR' },
        { 'id': '112', 'name': 'ZAM ZAM' },
        { 'id': '113', 'name': 'JHILL MILL' },
        { 'id': '114', 'name': 'SIVALINGAM SILK HOUSE' },
        { 'id': '115', 'name': 'VKC SILKS' },
        { 'id': '116', 'name': 'HARI SRI MURUGALAYA SILK HOUSE' },
        { 'id': '117', 'name': 'KUMARAN SILKS T-NAGAR' },
        { 'id': '118', 'name': 'YOGESH GARMENTS' },
        { 'id': '119', 'name': 'MOHAN READYMADES' },
        { 'id': '120', 'name': 'N PARVATHI' },
        { 'id': '121', 'name': 'APARANJI LONDON' },
        { 'id': '122', 'name': 'STEPIN' },
        { 'id': '123', 'name': 'VASAN SATTUR' },
        { 'id': '124', 'name': 'D STORE' },
        { 'id': '125', 'name': 'KOLANDA MUDALIYAR' },
        { 'id': '126', 'name': 'SIVANTHI' },
        { 'id': '127', 'name': 'SHALIHA' },
        { 'id': '128', 'name': 'VAIRA BOUTIQUE' },
        { 'id': '129', 'name': 'SANGAM' },
        { 'id': '130', 'name': 'HOT SPOT' },
        { 'id': '131', 'name': 'JKR TEX' },
        { 'id': '132', 'name': 'DCM' },
        { 'id': '133', 'name': 'ELEMECS' },
        { 'id': '134', 'name': 'SMNS SILKS' },
        { 'id': '135', 'name': 'SRI SATHYANARAYAN' },
        { 'id': '136', 'name': 'SPP SILKS' },
        { 'id': '137', 'name': 'HOT BOYS' },
        { 'id': '138', 'name': '90 ML' },
        { 'id': '139', 'name': 'PANDIAN TEXTILES' },
        { 'id': '140', 'name': 'REDDY GARMENTS' },
        { 'id': '141', 'name': 'ABC COTTON' },
        { 'id': '142', 'name': 'ANAND TEXTILE' },
        { 'id': '143', 'name': 'KSP - K.S PATTABIRAMA CHETTIAR SON' },
        { 'id': '144', 'name': 'KOODALINGAM' },
        { 'id': '145', 'name': 'KRISHNA TEXTILE (KTS)' },
        { 'id': '146', 'name': 'RANI READYMADES' },
        { 'id': '147', 'name': 'EXIMO' },
        { 'id': '148', 'name': 'TAQUA SILKS' },
        { 'id': '149', 'name': '99 STORE' },
        { 'id': '150', 'name': 'MENS CLUB' },
        { 'id': '151', 'name': 'BOLT' },
        { 'id': '152', 'name': 'ANANDHA SILKS' },
        { 'id': '153', 'name': 'NAMNADU' },
        { 'id': '154', 'name': 'MAYIL MURUGAN' },
        { 'id': '155', 'name': 'EMAYAM' },
        { 'id': '156', 'name': 'RAINBOW - HOSUR' },
        { 'id': '157', 'name': 'D-ZONE' },
        { 'id': '158', 'name': 'MUHARAT' },
        { 'id': '159', 'name': 'ABC MART' },
        { 'id': '160', 'name': 'SHA CLOTHING' },
        { 'id': '161', 'name': 'SHANMUGA SILKS' },
        { 'id': '162', 'name': 'DILLO' },
        { 'id': '163', 'name': 'SADHANA' },
        { 'id': '164', 'name': 'PODHIGAI' },
        { 'id': '165', 'name': 'GA BOYS' },
        { 'id': '166', 'name': 'MNPM' },
        { 'id': '167', 'name': 'SHOBA SINGAMPUNARI' },
        { 'id': '168', 'name': 'KAVERI TEXTILES' },
        { 'id': '169', 'name': 'AALIYAH' },
        { 'id': '170', 'name': 'LAKSHMI TRADERS' },
        { 'id': '171', 'name': 'VALLIMUTHU' },
        { 'id': '172', 'name': 'TAYLOR GANG' },
        { 'id': '173', 'name': 'UNISILKS' },
        { 'id': '174', 'name': 'SHIVA TEX' },
        { 'id': '175', 'name': 'SIMLA' },
        { 'id': '176', 'name': 'AKT TEXTILE' },
        { 'id': '177', 'name': 'SEEMATTI - TTP' },
        { 'id': '178', 'name': 'MAYURA' },
        { 'id': '179', 'name': 'SRI VELS SILKS' },
        { 'id': '180', 'name': 'ZAP' },
        { 'id': '181', 'name': 'AHIMSA PATTU MAHAL' },
        { 'id': '182', 'name': 'MURA SONS' },
        { 'id': '183', 'name': 'RAJ SHOPPING' },
        { 'id': '184', 'name': 'PREMIUM CLOTHING' },
        { 'id': '185', 'name': 'BUTTONS' },
        { 'id': '186', 'name': 'C.N.K READYMADES' },
        { 'id': '187', 'name': 'YES BHARATH' },
        { 'id': '188', 'name': 'AKSHAYA' },
        { 'id': '189', 'name': 'NAVANEETHA LALGUDI' },
        { 'id': '190', 'name': 'RAMACHANDRAN' },
        { 'id': '191', 'name': 'RAJA SILKS - THIRUVANNAMALAI' },
        { 'id': '192', 'name': 'SHIVA SILKS - NAMAKKAL' },
        { 'id': '193', 'name': 'KARTHIKA STORES' },
        { 'id': '194', 'name': 'MAHA SHAKTHI' },
        { 'id': '195', 'name': 'PONDY BAZZAR MENS' },
        { 'id': '196', 'name': 'SRI VINAYAKA CREATIONS 1' },
        { 'id': '197', 'name': 'SARANYA NAGAPPATTINAM' },
        { 'id': '198', 'name': 'MARLYNS' },
        { 'id': '199', 'name': 'AMARAVATHY SILKS AND READYMADES' },
        { 'id': '200', 'name': 'SRI VINAYAKA CREATIONS 2' },
        { 'id': '201', 'name': 'YASMEEN SILKS AND READYMADES' },
        { 'id': '202', 'name': 'SUMTHI READYMADES' },
        { 'id': '203', 'name': 'DEEPAK MATCHING CENTRE' },
        { 'id': '204', 'name': 'BHAIRAVI TEXTILES' },
        { 'id': '205', 'name': 'MOHAN TEXTILES' },
        { 'id': '206', 'name': 'ASTON GRABS' },
        { 'id': '207', 'name': 'BE QUEEN FASHIONS' },
        { 'id': '208', 'name': 'CHAKRAVARTHY THUHIL MAALIGAI' },
        { 'id': '209', 'name': 'THE PERFECT THREADS' },
        { 'id': '210', 'name': 'BLACK SPADE' },
        { 'id': '211', 'name': 'ATLAS' },
        { 'id': '212', 'name': 'HUSHPURR' },
        { 'id': '213', 'name': 'VASTHARA SILKS & READYMADES' },
        { 'id': '214', 'name': 'RASI READYMADES - PARAMAKUDI' },
        { 'id': '215', 'name': 'SRI KRISHNA SILKS - THENI' },
        { 'id': '216', 'name': 'AAKRUTI' },
        { 'id': '217', 'name': 'SIVAMEENAL ANDIPATTI' },
        { 'id': '218', 'name': 'BOUNCING BABY' },
        { 'id': '219', 'name': 'DAZZLING BEAUTY' },
        { 'id': '220', 'name': 'KANISHQ SILKS AND READYMADES' },
        { 'id': '221', 'name': 'THILLAI WEDDING SILKS' },
        { 'id': '222', 'name': 'HONGKONG CUT PIECE AND READYMADES' },
        { 'id': '223', 'name': 'SEQUENCE' },
        { 'id': '224', 'name': 'SHECRETS' },
        { 'id': '225', 'name': 'SALAMMAL' },
        { 'id': '226', 'name': 'ROOPALI TEXTILES' },
        { 'id': '227', 'name': '2K MEN' },
        { 'id': '228', 'name': 'MY LITTLE ONE' },
        { 'id': '229', 'name': 'PACHIYAPPAS SILKS' },
        { 'id': '230', 'name': 'BIGG BOSS (FABULAS)' },
        { 'id': '231', 'name': 'SRI KRISHNA SILKS (THIRUPATHUR)' },
        { 'id': '232', 'name': 'GRACE FASHION (AAKRUTI)' },
        { 'id': '233', 'name': 'RAJAN TEXTILES (THIRUVANNAMALAI)' },
        { 'id': '234', 'name': 'SANGEETHA SILKS' },
        { 'id': '235', 'name': 'FOCUS MENSWEAR' },
        { 'id': '236', 'name': 'BIG DRESSES' },
        { 'id': '237', 'name': 'SINGAL FABRICES' },
        { 'id': '238', 'name': 'RATHNA SILKS' },
        { 'id': '239', 'name': 'MADURA SILKS' },
        { 'id': '240', 'name': 'NEW MASS READYMADES' },
        { 'id': '241', 'name': 'JAYVANTH' },
        { 'id': '242', 'name': 'RR JUNCTION' },
        { 'id': '243', 'name': 'RK WEDDING MALL' },
        { 'id': '244', 'name': 'NAMMA PRIYAZ' },
        { 'id': '245', 'name': 'PALANIYAPPA TEXTILE' },
        { 'id': '246', 'name': 'K CHINNADURAI' },
        { 'id': '247', 'name': 'SUMANGALI SILKS - MYSORE' },
        { 'id': '248', 'name': 'SG SHOPPING MALL' },
        { 'id': '249', 'name': 'KRISHNA LIFE STYLE' },
        { 'id': '250', 'name': 'NETHRA APPAREL' },
        { 'id': '251', 'name': 'MM SILKS' },
        { 'id': '252', 'name': 'DEMOZA' },
        { 'id': '253', 'name': 'MY COLORS' },
        { 'id': '254', 'name': 'SHIVA TEXTILES - COIMBATORE' },
        { 'id': '255', 'name': 'MODERN FAMILY SHOP' },
        { 'id': '256', 'name': 'MAHARAJA READYMADES - KARAIKUDI' },
        { 'id': '257', 'name': 'MAHARAJA SILKS - KARAIKUDI' },
        { 'id': '258', 'name': 'ADITYA TEXTILES' },
        { 'id': '259', 'name': 'ARUNA SILKS - PULIYANGUDI' },
        { 'id': '260', 'name': 'JAYA SILKS - CHITTOR' },
        { 'id': '261', 'name': 'VARUN SILKS - USLAMPATTI' },
        { 'id': '262', 'name': 'SELVA GANESH - UDUMALPET' },
        { 'id': '263', 'name': 'KRISHNA SILKS MYSORE - GUNDULPET' },
        { 'id': '264', 'name': 'SUMEETH STORES - CHATTISGARH' },
        { 'id': '265', 'name': 'GRAND HOUSE - KERALA' },
        { 'id': '266', 'name': 'BHARATHI SIVA SILK - NEMELI' },
        { 'id': '267', 'name': 'MANGALYAM SILKS - CHIDAMBARAM' }
        ];

        $scope.Entity = {
            "id": 0,
            "companyid": 0,
            "locationid": 0,
            "typeid": 0,
            "priorityid": 0,
            "statusid": 0,
            "requestedby": null,
            "requestedon": new Date(),
            "contactno": null,
            "bugdetails": null,
            "attachment": null,
            "assignedby": 0,
            "assignedon": new Date(),
            "resolvedby": 0,
            "resolvedon": new Date(),
            "isactive": true,
            "version": 1,
            "modifiedby": 0,
            "createdby": 0,
            "createdon": new Date(),
            "modifiedon": new Date(),
            "bugno": null
        };

        $scope.BugTracker = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.BugTracker = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.AddSlide = function () {
            if ($scope.BugTracker.attachment == 'undefined' || $scope.BugTracker.attachment == null)
                $scope.BugTracker.attachment = [];
            $scope.BugTracker.attachment.push($scope.Slide);
            $scope.Slide = { 'title': '', 'image': '' };
        };
        $scope.ManagesearchSlideItem = function (action, item) {
            if (action == "DeleteSlide") {
                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ' + item.name + "?",
                    confirmButton: 'Yes',
                    confirmButtonClass: 'btn-info',
                    icon: 'fa fa-question-circle',
                    animation: 'scale',
                    animationClose: 'top',
                    opacity: 0.5,
                    buttons: {
                        formSubmit: {
                            text: 'Submit',
                            btnClass: 'btn-blue',
                            action: function () {
                                $scope.BugTracker.attachment.splice($scope.BugTracker.attachment.indexOf(item), 1);
                                $scope.$apply();
                            }
                        },
                        cancel: function () {
                            //close
                        },
                    },

                    onContentReady: function () {
                        // bind to events
                        var jc = this;
                        jc.$$formSubmit.trigger('focus');
                        this.$content.find('form').on('submit', function (e) {
                            // if the user submits the form by pressing enter in the field.
                            e.preventDefault();
                            jc.$$formSubmit.trigger('click'); // reference the button and click it
                        });
                    }
                });


            }
            else if (action == "EditSlide") {
                $scope.Slide = item;
            }
        }

        $scope.SearchCondition = {
            "companyid": 0,
            "typeid": 0,
            "priorityid": 0,
            "statusid": 1
        }

        $scope.New = function () {
            $scope.BugTracker = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.Save = function () {

            if ($scope.BugTracker.companyid == null || $scope.BugTracker.companyid == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please Select Company Name");
                return;
            }
            if ($scope.BugTracker.typeid == null || $scope.BugTracker.typeid == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please Select Bug Type");
                return;
            }
            if ($scope.BugTracker.priorityid == null || $scope.BugTracker.priorityid == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please Select Bug Priority");
                return;
            }
            if ($scope.BugTracker.statusid == null || $scope.BugTracker.statusid == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please Select Bug Status");
                return;
            }
            if ($scope.BugTracker.requestedby == null || $scope.BugTracker.requestedby == '') {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please Enter the Name of the person who raised the request");
                return;
            }
            if ($scope.BugTracker.bugdetails == null || $scope.BugTracker.bugdetails == '') {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please Enter the bug details");
                return;
            }
            request = {
                "Item": $scope.BugTracker,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BugTrackerService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {

                    $rootScope.Toaster.Info("BugTracker Saved Successfully.!");
                    $scope.BugTracker = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {

                    $rootScope.Toaster.Error(result.ErrorMessage);

                }

            }).error(function (result) {
            });

        }


        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {


                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ' + item.name + "?",
                    confirmButton: 'Yes',
                    confirmButtonClass: 'btn-info',
                    icon: 'fa fa-question-circle',
                    animation: 'scale',
                    animationClose: 'top',
                    opacity: 0.5,
                    buttons: {
                        formSubmit: {
                            text: 'Submit',
                            btnClass: 'btn-blue',
                            action: function () {
                                request = {
                                    "Item": item,
                                    "SecurityToken": "String content"
                                }
                                var srequest = JSON.stringify(request);
                                $http({
                                    method: 'POST',
                                    url: "BugTrackerService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                                    }
                                }).error(function (result) {

                                });
                            }
                        },
                        cancel: function () {
                            //close
                        },
                    },

                    onContentReady: function () {
                        // bind to events
                        var jc = this;
                        jc.$$formSubmit.trigger('focus');
                        this.$content.find('form').on('submit', function (e) {
                            // if the user submits the form by pressing enter in the field.
                            e.preventDefault();
                            jc.$$formSubmit.trigger('click'); // reference the button and click it
                        });
                    }
                });


            }
            else if (action == "Edit") {
                $scope.ShowSearch = false;
                $scope.BugTracker = item;
                $scope.Navigation.SetMode('EDIT');
            }
        }

        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BugTrackerService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.BugTrackerConfig();
                $scope.igrid.UpdateData($scope.SearchResult, '_id');
            }).error(function (result) {
            });
        }

        $scope.BugTrackerConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '500px', '100%', false, false, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('bugno', '#Bug No', 60);
            igrid.AddReferenceColumn('companyid', 'Customer', 140, $scope.Company);
            igrid.AddReferenceColumn('typeid', 'Type', 100, $scope.Type);
            igrid.AddReferenceColumn('priorityid', 'Priority', 80, $scope.Priority);
            igrid.AddReferenceColumn('statusid', 'Status', 80, $scope.Status);
            igrid.AddSimpleColumn('requestedby', 'Requested By', 150);
            igrid.AddFormatedColumn('requestedon', 'Requested On', 100, 'dd/MM/yyyy');
            igrid.AddSimpleColumn('contactno', 'Contact No', 100);
            igrid.AddSimpleColumn('bugdetails', 'Bug Detail', 250);
            igrid.AddReferenceColumn('assignedby', 'Assigned To', 100, $scope.Employee);

            var commands = "";
            commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"

            igrid.AddButtonColumn('action', 'ACTION', 50, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Edit')
                        $scope.ManageSearchItem('Edit', args.item);
                });
            });
            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult, '_id');
        }

        $timeout(function () {
            $scope.setFocusfirst();
            $scope.BugTrackerConfig();
            if (typeof ($location.search().id) != 'undefined' &&
                $location.search().id.length > 0) {
                request = {
                    "Item": { 'id': $location.search().id },
                    "SecurityToken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "BugTrackerService/Search",
                    data: srequest
                }).success(function (result) {

                    if (angular.isArray(result)) {
                        $scope.ShowSearch = false;
                        $scope.BugTracker = result[0];
                    }

                }).error(function (result) {
                });
            }
        }, 100);

    }]);