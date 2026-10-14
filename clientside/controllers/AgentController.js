
var app = angular.module('InfinityLab');

app.controller('AgentController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService','$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;

        $scope.Banks = restService.Get('ReferencelistService/BANK', true).then(function (results) {
            $scope.Banks = results;
        });


        $scope.Cities = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.Cities = results;
        });

        $scope.AgentTypes = [{ 'id': 1, 'name': 'Local' },
            { 'id': 2, 'name': 'InterState' },
            { 'id': 3, 'name': 'International' },
            { 'id': 4, 'name': 'Referral' }];


        $scope.TaxList = restService.Get('ReferencelistService/PURCHASETAX', true).then(function (results) {
            $scope.TaxList = results;
        });

        $scope.States = restService.Get('ReferencelistService/STATE', true).then(function (results) {
            $scope.States = results;
        });

        $scope.Countries = restService.Get('ReferencelistService/COUNTRY', true).then(function (results) {
            $scope.Countries = results;
        });


        $scope.StatusList = [{ id: 1, name: 'Unapproved' }
            , { id: 2, name: 'Approved' },
            { id: 3, name: 'Deactivated' }];

        $scope.Entity = {
            "id": 0,
            "name": "",
            "contactperson": "",
            "contactno": "",
            "address": "",
            "emailid": "",
            "cityid": 0,
            "stateid": 0,
            "countryid": 0,
            "pincode": "",
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "",
            "modifiedon": "",
            pan: "",
            gst: "",
            commissionamount: "",
            commissionpercentage: "",
            agenttypeid:""
        };

        $scope.Agent = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Agent = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ErrorMessage = "";



        $scope.SearchCondition = {
            name: "",
            contactperson: ""
        }

        $scope.New = function () {
            $scope.Agent = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ErrorMessage = "";
        }

        $scope.Save = function () {

            if ($scope.Agent.name == null || $scope.Agent.name == 0)
            {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please enter Agent Name");
                return;
            }
            if ($scope.Agent.contactperson == null || $scope.Agent.contactperson == 0) {
                $scope['setFocuscontactperson']();
                $rootScope.Toaster.Error("Please enter contact person name");
                return;
            }
            if ($scope.Agent.contactno == null || $scope.Agent.contactno == 0) {
                $scope['setFocuscontactno']();
                $rootScope.Toaster.Error("Please enter contact no");
                return;
            }

            request = {
                "Item": $scope.Agent,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "AgentService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                
                    $rootScope.Toaster.Info("Agent Saved Successfully.!");
                    $scope.Agent = (JSON.parse(JSON.stringify($scope.Entity)));
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
                                    url: "AgentService/Delete",
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
                $scope.Agent = item;
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
                url: "AgentService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.AgentConfig();
            }).error(function (result) {
            });

        }

        $scope.ShowStatusDialog = function () {

            var dialog = ngDialog.open({
                template: 'ChangeStatusDialog',
                data: [$scope],
                width: '400px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.parent = $scope.ngDialogData[0];
                    $scope.ChangeStock = function () {
                        $scope.closeThisDialog($scope.statusid);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$escape' && data.value != '$document') {
                        var request = { 'statusid': data.value, ids: [] };

                        $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                            request.ids.push(v.id);
                        });

                        var webrequest = {
                            "item": request,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(webrequest);
                        $http({
                            method: 'POST',
                            url: "AgentService/ChangeStatus",
                            data: srequest
                        }).success(function (result) {
                            $rootScope.Toaster.Error('Attributes successfully changed.');
                        }).error(function (result) {
                        });
                    }
                }
            });
        }

        $scope.exporttoExcel = function () {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'Agents';
            $scope.igrid.ExportXLSX($scope.igrid.grid, title, subtitle);
        }

        $scope.AgentConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('name', 'Name', 150);
            igrid.AddSimpleColumn('contactperson', 'Contact Person', 150);
            igrid.AddSimpleColumn('contactno', 'Contact No', 100);
            igrid.AddSimpleColumn('emailid', 'Email Id', 150);
            igrid.AddSimpleColumn('address', 'Address', 150);
            igrid.AddReferenceColumn('cityid', 'City', 100, $scope.Cities);
            igrid.AddReferenceColumn('stateid', 'State', 100, $scope.States);
            igrid.AddReferenceColumn('countryid', 'Country', 100, $scope.Countries);

            var commands = "";
            if ($rootScope.UserInfo.p_AGENTMASTER.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_AGENTMASTER.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            if ($rootScope.UserInfo.p_AGENTMASTER.modify || $rootScope.UserInfo.p_AGENTMASTER.delete) {
                igrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
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
            if (typeof ($location.search().id) != 'undefined' &&
              $location.search().id.length > 0) {
                request = {
                    "Item": { 'id': $location.search().id },
                    "SecurityToken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "AgentService/Search",
                    data: srequest
                }).success(function (result) {

                    if (angular.isArray(result)) {
                        $scope.ShowSearch = false;
                        $scope.Agent = result[0];
                    }

                }).error(function (result) {
                });
            }
        }, 100);

    }]);