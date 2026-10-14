var app = angular.module('InfinityLab');

app.controller('OrganisationController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;

        $scope.Cities = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.Cities = results;
        });
        $scope.States = restService.Get('ReferencelistService/STATE', true).then(function (results) {
            $scope.States = results;
        });
        $scope.Countries = restService.Get('ReferencelistService/COUNTRY', true).then(function (results) {
            $scope.Countries = results;
        });

        $scope.CreateDB = function () {
            restService.Get('OrganisationService/CreateDB/' + $scope.Organisation.id, true).then(function (results) {
                alert(results);
            });
        }


        $scope.CreateUsers = function () {
            restService.Get('OrganisationService/CreateUsers/' + $scope.Organisation.id, true).then(function (results) {
                alert(results);
            });
        }

        

        $scope.Entity = {
            code:"",
            name: "",
            contactperson: "",
            contactno: "",
            emailid: "",
            address: "",
            cityid: "",
            stateid: "",
            countryid: "",
            pincode: "",
            isactive: true,
            nooflocation: 0,
            noofusers: 0,
            remarks: "",
            subscriptionfees: 0
        };
        $scope.Organisation = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Organisation = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ErrorMessage = "";
        $scope.SearchCondition = {
            name: "",
            contactperson: ""
        }

        $scope.New = function () {
            $scope.Organisation = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.IsEmpty = function (value) {
            return typeof (value) == 'undefined' || value == '';
        }
        $scope.Save = function () {

            if ($scope.IsEmpty($scope.Organisation.code)) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please fill out organisation code");
                return;
            }
            if ($scope.IsEmpty($scope.Organisation.name)) {
                $scope['setFocusname']();
                $rootScope.Toaster.Error("Please fill out organisation name");
                return;
            }
            request = {
                "Item": $scope.Organisation,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "OrganisationService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && (result.ErrorMessage == null || result.ErrorMessage == '') && result.id > 0) {
                    $rootScope.Toaster.Info("Organisation Saved Successfully.!");
                    $scope.Organisation = (JSON.parse(JSON.stringify($scope.Entity)));
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
                                    url: "OrganisationService/Delete",
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
                $scope.Organisation = item;
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
                url: "OrganisationService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.OrganisationConfig();
            }).error(function (result) {
            });

        }

        $scope.OrganisationConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '420px', '100%', true, false, true, 0);
            igrid.AddSimpleColumn('code', 'Code', 100);
            igrid.AddSimpleColumn('name', 'Name', 200);
            igrid.AddSimpleColumn('contactperson', 'Contact Person', 150);
            igrid.AddSimpleColumn('contactno', 'Contact No', 140);
            igrid.AddSimpleColumn('emailid', 'Email Id', 150);
            igrid.AddSimpleColumn('address', 'Address', 200);
            igrid.AddReferenceColumn('cityid', 'City', 100, $scope.Cities);

            var commands = "";
            if ($rootScope.UserInfo.p_ORGMASTER.modify) {
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Migrate DB'/> &nbsp;"
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Synch DB Structure'/> &nbsp;"
            }

            if ($rootScope.UserInfo.p_ORGMASTER.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            if ($rootScope.UserInfo.p_ORGMASTER.modify || $rootScope.UserInfo.p_ORGMASTER.delete) {
                igrid.AddButtonColumn('action', 'ACTION', 500, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Edit')
                            $scope.ManageSearchItem('Edit', args.item);
                        else if ($(e.target).val() == 'Delete')
                            $scope.ManageSearchItem('Delete', args.item);
                    });
                });

            }
            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);

        }


        $timeout(function () {
            $scope.setFocusfirst();
            $scope.OrganisactionConfig();
        }, 10);

    }]);
