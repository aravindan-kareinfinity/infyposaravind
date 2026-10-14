
var app = angular.module('InfinityLab');
app.controller('HomePageController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$timeout',
function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $timeout) {
    $rootScope.AccordionName = $location.path();
    $scope.SearchRef = restService.SearchReference;
    $scope.primaryinstitutionid = UserDetails.primaryinstitutionid;
    $scope.logininstitutionid = UserDetails.institutionid;

    $(document).ready(function () {
        $('#summernote').summernote({
            height: 135,
            minHeight: null,
            maxHeight: null,
            focus: true
        });
    });
    $(document).ready(function () {
        $('#summernote_1').summernote({
            height: 135,
            minHeight: null,
            maxHeight: null,
            focus: true
        });
    });


    $scope.Entity = {
        "id": 0,
        "isactive": true,
        "createdby": 0,
        "modifiedby": 0,
        "version": 1,
        "createdon": new Date(),
        "modifiedon": new Date(),
        "schoolname": "",
        "domain": "",
        "institutionid": $scope.logininstitutionid,
        //"contactus": { "mobilenumber ": "", "emailid ": "", "address ": "" },
        "mobilenumber": [],
        "emailid": [],
        "address":[],
        "facilities": [],
        "slides": [],
        "images": [],
        "aboutus": [],
        "logo": [],
        "isselected": true,
        "ctitle": "",
        "ititle": "",
        "ftitle": "",
        "systemimages":[]
    };

    $scope.Homepage = (JSON.parse(JSON.stringify($scope.Entity)));
    $scope.Slide = { 'title': '', 'subtitle': '', 'image': '' };
    $scope.Facilitie = { 'heading': '', 'description': '', 'icon': '' };
    $scope.Content = { 'title': '', 'display': '' };
    $scope.ErrorStyle = "success";
    $scope.ImageID = { 'image': '' };
    $scope.Systemimage = { 'title': '', 'image': '', 'imagepath': '' };
    
    $scope.AddSystemimage = function () {
        if ($scope.Homepage.systemimages == null)
            $scope.Homepage.systemimages = [];

        $scope.Homepage.systemimages.push($scope.Systemimage);
        $scope.Systemimage = { 'image': '', 'imagepath': '' };
    };

    $scope.AddImageID = function () {
        $scope.Homepage.images.push($scope.ImageID);
        $scope.ImageID = { 'image': '' };
    };
    $scope.AddSlide = function () {
        $scope.Homepage.slides.push($scope.Slide);
        $scope.Slide = { 'title': '', 'subtitle': '', 'image': '' };
    };

    $scope.AddFacilitie = function () {
        $scope.Facilitie.description = $('#summernote_1').summernote('code');
        $('#summernote_1').summernote('reset');
        $scope.Homepage.facilities.push($scope.Facilitie);
        $scope.Facilitie = { 'heading': '', 'description': '', 'icon': '' };
    };

    $scope.AddContent = function () {
        $scope.Content.display = $('#summernote').summernote('code');
        $('#summernote').summernote('reset');
        $scope.Homepage.aboutus.push($scope.Content);
        $scope.Content = { 'title': '', 'display': '' };
    }


    $scope.Save = function () {

        request = {
            "Item": $scope.Homepage,
            "securitytoken": "String content"
        }

        var srequest = JSON.stringify(request);

        $http({
            method: 'POST',
            url: "HomepageService/Save",
            data: srequest
        }).success(function (result) {

            if (result != null && result.id > 0) {
                $rootScope.Toaster.Info("Successfully saved");
                $scope.Homepage = (JSON.parse(JSON.stringify($scope.Entity)));
                $scope.ErrorStyle = "success";
                
                //$scope.Slide.push($scope.Homepage.slides);
               // $scope.Facilitie.push($scope.Homepage.facilities);
               
            }
            else {
                $scope.ErrorMessage = result.ErrorMessage;
                $scope.ErrorStyle = "danger";
            }

        }).error(function (result) {
        });

    }


    $scope.ManageSearchItem = function (action, item) {
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
                            $scope.Homepage.slides.splice($scope.Homepage.slides.indexOf(item), 1);
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
        else if (action == "DeleteImageID") {

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
                            $scope.Homepage.images.splice($scope.Homepage.images.indexOf(item), 1);
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
        else if (action == "EditImageID") {
            $scope.ImageID = item;

        }

        else if (action == "DeleteSystemimage") {

            $.confirm({
                title: 'Delete',
                content: 'Do you want to delete ' + item.image + "?",
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
                            $scope.Homepage.systemimages.splice($scope.Homepage.systemimages.indexOf(item), 1);
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
        else if (action == "EditSystemimage") {
            $scope.Systemimage = item;

        }


        else if (action == "DeleteFacilitie") {

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
                            $scope.Homepage.facilities.splice($scope.Homepage.facilities.indexOf(item), 1);
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
        else if (action == "EditFacilitie") {
            $('#summernote_1').summernote('reset');
            $scope.Facilitie = item;
            $('#summernote_1').summernote('code', item.description);
            //$scope.Facilitie = item;

        }

        else if (action == "DeleteContent") {

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
                            $scope.Homepage.aboutus.splice($scope.Homepage.aboutus.indexOf(item), 1);
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
        else if (action == "EditContent") {
            $('#summernote').summernote('reset');
            $scope.Content = item;
            $('#summernote').summernote('code', item.display);
        }
    }

   
        var request = {
            "Item": { 'institutionid': UserDetails.institutionid },
            "securitytoken": "String content"
        }

        var srequest = JSON.stringify(request);

        $http({
            method: 'POST',
            url: "HomepageService/Search",
            data: srequest
        }).success(function (result) {
            if (result.length >0)
                $scope.Homepage = result[result.length-1];
        }).error(function (result) {
        });

   

}]);
