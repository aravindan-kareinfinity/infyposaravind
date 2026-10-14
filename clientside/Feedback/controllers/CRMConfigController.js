//ReferencelistService/CITY
var app = angular.module('InfinityLab');

app.controller('CRMConfigController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $scope.Navigation = $rootScope.GetNavigation();

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }


        $scope.SearchRef = restService.SearchReference;

        $scope.Entity = {
            code: "",
            name: "",
            contactperson: "",
            contactno: "",
            address: "",
            isactive: true,
            version: 1,
            branches: [],
            html: {},
            fld_title: 'Customer Loyalty Program Form',
            fld_mobileno : 'Mobile No',
            fld_firstname: 'First Name',
            fld_lastname: 'Last Name',
            fld_mailid: 'Mail id',
            fld_addressline1: 'Address',
            fld_addressline2: '',
            fld_area: 'Area',
            fld_city : 'City',
            fld_state : 'State',
            fld_postcode : 'Postal Code',
            fld_dob: 'Date of birth',
            fld_religion: 'Religion',
            fld_marital: 'Marital status',
            fld_sp_firstname: 'Spouse First Name',
            fld_sp_lastname: 'Spouse Last Name',
            fld_sp_dob: 'Spouse Date of birth',
            fld_dom: 'Date of Marriage',
            fld_source: 'Where did you hear about this?',
            fld_often: 'How often do you purchase?',
            fld_gender: 'Gender',
            fld_termsandcondition:'By clicking submit, you agree to our terms & condition and to receive promotional updates from '
        };
        $scope.Organization = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];


        $scope.SearchCondition = {
            name: "",
            contactperson: ""
        }


        $scope.IsEmpty = function (value) {
            return typeof (value) == 'undefined' || value == '';
        }
        $scope.Save = function () {

            var srequest = JSON.stringify({ 'Item': $scope.Organization });

            $http({
                method: 'POST',
                url: "CRMConfigService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null) {
                    $rootScope.Toaster.Info("Feedback form Configuration Saved Successfully.!");
                    $scope.Organization.version = result.version;
                    $scope.Organization.modifiedon = result.modifiedon;
                    $scope.Organization.modifiedby = result.modifiedby;
                }


            }).error(function (result) {
            });

        }

        $scope.AddBranch = function () {
            if ($scope.Organization.branches == null) $scope.Organization.branches = [];
            $scope.Organization.branches.push({ 'code': $scope.branchcode, 'name': $scope.branchname });
        }

        $scope.DeleteBranch = function (item) {
            $scope.Organization.branches.splice($scope.Organization.branches.indexOf(item), 1);
        }

        $scope.SearchCurrent = function () {
            $http({
                method: 'GET',
                url: "CRMConfigService/Current"
            }).success(function (result) {
                if (result && result != null) {
                    $scope.Organization = result;
                }
            }).error(function (result) {
            });
        }

        $scope.New = function () {
            $scope.Organization = (JSON.parse(JSON.stringify($scope.Entity)));
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
                                    url: "CRMConfigService/Delete",
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
                $scope.Navigation.SetMode('EDIT');
                $scope.Organization = item;
            }
        }


        $scope.SearchCurrent();
    }
])
