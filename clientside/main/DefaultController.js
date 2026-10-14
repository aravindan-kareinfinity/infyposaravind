'use strict';
var app = angular.module('InfinityLab');
app.controller('DefaultController', ['$scope', '$location', '$http', '$rootScope', '$timeout', '$sce',
    function ($scope, $location, $http, $rootScope, $timeout, $sce) {
 
     // $rootScope.title = 'Popin Software Pvt Ltd.,';
    $scope.primaryinstitutionid = 0;
    $scope.Data = {};
    $scope.Data.schoolname = 'Popin Software Pvt Ltd.,';
     //$scope.Data.whoweare = 'We believe that learning can be inspired by almost anything and everything around children. Nature, people, colors, technology are all a part of this learning and we bring this holistic & refreshing approach into our classrooms.';
    //$scope.VisionStatemet = 'Our Mission is to nurture the children of this world in bringing  out the real leader in him/her, who are the hopes for the future of this world.';
    // $scope.MissionStatemet = 'Our Vision is to become one of the admirable organisation in this world in spreading the values of love, honesty, fun, happiness through our children.Our aim is to promote heart-based education for every one all over the world.';
    $scope.Data.ctitle = 'Contact';
    $scope.Data.ititle= 'Image Gallery';
    $scope.Data.ftitle = 'Our Facilities';
   // $scope.Data.facilities = 'School Management Software';
    $scope.Data.emailid = 'popinsoftware@gmail.com';
    $scope.Data.address = 'No:42/1, Harinivasa Paradise, Basement Floor, Hosur Main Road, Bande Palya, Bangalore – 560068, India';
    $scope.Data.mobilenumber = '04142';
    $scope.Data.logo = 'ClientSide/images/favicon.png';
    $scope.Data.slides = [
        {
            'image': '..//..//ClientSide/images/school1.jpg',
            'title': 'Popin Software Pvt Ltd.,',
            'subtitle': 'School Managemet Software'

        },
        {
            'image': '..//..//ClientSide/images/school2.jpg',
            'title': 'Popin Software Pvt Ltd.,',
            'subtitle': 'School Managemet Software'
        },
        {
            'image': '..//..//ClientSide/images/school3.jpg',
            'title': 'Popin Software Pvt Ltd.,',
            'subtitle': 'School Managemet Software'
        }

    ];
    // $scope.slide.push($scope.slide);
    // $scope.myHTML = $sce.trustAsHtml('This is an <b>html </b>string with ' + '<a href="">url!</a>');

    $scope.Data.aboutus = [
        {
            'isdefault':true,
            'title':'Who We Are',
            'display': 'We believe that learning can be inspired by almost anything and everything around children. Nature, people, colors, technology are all a part of this learning and we bring this holistic & refreshing approach into our classrooms.',
        },
    {
        'isdefault': false,
        'title': 'Vision Statement',
        'display': 'Our Vision is to become one of the admirable organisation in this world in spreading the values of love, honesty, fun, happiness through our children.Our aim is to promote heart-based education for every one all over the world.',
    },
    {
        'isdefault': false,
        'title': 'Mission Statement',
        'display': 'Our Mission is to nurture the children of this world in bringing  out the real leader in him/her, who are the hopes for the future of this world. differentiated, in-depth and cohesive learning programs aligned to year level content and achievement standards informed by the Australian curriculumhighly effective teachers, focussed on improving student outcomes, through their commitment to ongoing professional development, quality teaching, evidence based practices, coaching and mentoring and collaboration.'
    }
    ];


   
    $scope.Data.facilities = [
            {
                'icon': 'ClientSide/images/board.png',
                'heading': 'Inside our Classroom',
                'description': 'A spacious classroom of over 500 sq.ft, awaits children. Wide doors & windows, high ceiling give a spacious feel which stimulates learning.With only 25 children in KG & 30 in other classes you can expect personal attention to the children. Our attention to detail has created a comfortable and healthy environment for every child.Every classroom is an interactive digital class with the teacher using all three types of teaching aides to teach visual, auditory and kinesthetic learners.'
            },

            {
                'icon': 'ClientSide/images/bus.png',
                'heading': 'Around the campus',
                'description': 'We have conserved every tree inside the campus as we built the school and have made it an integral part of the school. Shrubs, plants and trees are a part of our building including the terrace to bring nature indoors. The terrace has an open air amphitheater to engage children in performing arts in a natural setting.A well designed and safe play area in the kindergarten section is an added treat to our children. Several sandpits, jungle gyms with padded floors, water puddles make this space interesting.'
            },
            {
                'icon': 'ClientSide/images/book.png',
                'heading': 'Technology',
                'description': 'To inculcate reading habit our library is available with a wide variety of books alongside content filtered internet terminals. Fully integrated and equipped labs for math & science is available.We have tied up with robotic telescopes located across the world, allowing children to capture pictures & videos of space objects in real time adding a new dimension to learning.LED notice boards populate the school. Students will be able to view their drawings, paintings, movies, photos, news, time etc., on these notice boards.'
            },
            {
                'icon': 'ClientSide/images/chair.png',
                'heading': 'Classroom Size and Space',
                'description': 'Overcrowded classrooms—and schools—have consistently been linked to increased levels of aggression in students. Overcrowded classrooms are also associated with decreased levels of student engagement and, therefore, decreased levels of learning..'

            },
            {
                'icon': 'ClientSide/images/cup.png',
                'heading': 'Sports Facilities',
                'description': 'Along with academics, physical fitness is given importance.With an eye to groom students into physically strong and mentally active members, a range of sports facilities are made available.This include two basketball courts, a football field, a cricket pitch, badminton volleyball and tennis courts and a  playground with synthetic track.',

            },

            {
                'icon': 'ClientSide/images/scale.png',
                'heading': 'Books and Uniforms Store',
                'description': 'The school houses a well stocked store, which offers text books, note books, uniforms and a range of miscellaneous items at special prices.The store stocks all prescribed text books, stationery and uniforms for the convenience of students.',

            },

            
    ];
    
    $scope.Data.images = [
            { image: 'ClientSide/images/black.png' },
            { image: 'ClientSide/images/sport.png' },
            { image: 'ClientSide/images/tech.png' }
    ];
    
    

    var domain = 'Poin';
    var domainParts = window.location.hostname.toUpperCase().split('.');
    if (domainParts.length == 3 && domainParts[1] == "SCHOOL2MANAGE") {
        domain = domainParts[0];
    }

    var request = {
        "item": { 'domain': domain },
        "securitytoken": "String content"
    }

    var srequest = JSON.stringify(request);

    $http({
        method: 'POST',
        url: "HomepageService/Search",
        data: srequest
    }).success(function (result) {

        $scope.primaryinstitutionid = result[0].institutionid;

        result[0].logo = $scope.applyImagePath(result[0].logo);
        $.each(result[0].slides, function (k, v) {
            v.image = $scope.applyImagePath(v.image);

        });

        $.each(result[0].facilities, function (k, v) {
            v.icon = $scope.applyImagePath(v.icon);
            v.description = $sce.trustAsHtml(v.description);
        });

        //$.each(result[0].aboutus, function (k, v) {
        //    v.display = $sce.trustAsHtml(v.display);
        //});

        if (result[0].aboutus != null && result[0].aboutus.length > 0) {
            var defaultaboutus = null;
            $.each(result[0].aboutus, function (k, v) {
                v.display = $sce.trustAsHtml(v.display);
                if (v.isdefault && defaultaboutus == null)
                    defaultaboutus = v;
            });

            if (defaultaboutus == null)
                defaultaboutus = result[0].aboutus[0];

            result[0].aboutus.splice(result[0].aboutus.indexOf(defaultaboutus), 1);
            result[0].welcome = defaultaboutus;
        }


        result[0].image_1 = [];
        result[0].image_2 = [];
        result[0].image_3 = [];
        $.each(result[0].images, function (k,v) {
            v.image = $scope.applyImagePath(v.image);
            switch(k%3)
            {
                case 0:
                    result[0].image_1.push(v);
                    break;
                case 1:
                    result[0].image_2.push(v);
                    break;
                case 2:
                    result[0].image_3.push(v);
                    break;
            }
        });
        $scope.Data = result[0];

        $timeout(function () {
            jQuery(document).ready(function () {
                revapi = jQuery('.tp-banner').revolution({
                    delay: 7000,
                    startwidth: 1170,
                    startheight: 500,
                    hideThumbs: 10,
                    fullWidth: "off",
                    fullScreen: "on",
                    fullScreenOffsetContainer: ""
                });
            });

            try {

                var SliderPost = $('.flexslider');

                SliderPost.flexslider({
                    animation: "swing",
                    slideshowSpeed: 4000,
                });
            } catch (err) {

            }
        }, 100);


    }).error(function (result) {
    });

    $scope.applyImagePath = function(imageid)
    {
        return 'File/Show/' + $scope.UserInfo.OrganizationID + '/' + imageid;
    }
    //var defaultdisplay = $scope.Data.Display.find(function (item) {
    //    return item.isdefault;
    //});

    //if (typeof (defaultdisplay) != 'undefined' && defaultdisplay != null) {
    //    $scope.Data.firstDisplay = defaultdisplay;
    //    $scope.Display.splice($scope.Data.Statements.indexOf(defaultdisplay), 1);
    //}

    var revapi = null;
    

}]);