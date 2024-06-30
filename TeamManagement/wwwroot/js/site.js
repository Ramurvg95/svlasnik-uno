// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// search text box enter click

$(document).ready(function () {

    $('#searchtextbox').keypress(function (e) {
        if (e.keyCode === 13) {
            searchResults();
        }
    });
});

//search for members

function searchResults() {
    var value = document.getElementById('searchtextbox').value;
    var url = "/Home/FindUsers?searchText=" + value;
    $.ajax(url,
        {
            type: 'Get',
            contentType: 'application/json; charset=utf-8',

            success: function (recData) {
                var divElement = document.getElementById('userResults');
                $(divElement).empty().html(recData);
            }
        });
}

//User selection to include in team

function onCheckboxCheckChanged(user) {
    let element = user;
    let id = element.id;

    var selectedUserIds = $('#selectedUserIds').val();
    var selectedUsersArray = selectedUserIds.split(",");

    //Remove the 0th element
    if (selectedUsersArray.length > 0 && selectedUsersArray[0] == "") {
        selectedUsersArray.shift();
    }

    if (element.checked) {
        if (!selectedUsersArray.includes(id, 0)) {
            selectedUsersArray.push(id);
        }
    }
    else {
        if (selectedUsersArray.includes(id, 0)) {
            let idx = selectedUsersArray.indexOf(id);
            selectedUsersArray.splice(idx, 1);
        }
    }

    $('#selectedUserIds').val(selectedUsersArray);
}

//saves new team

function addTeam() {
    var teamName = document.getElementById('TeamName').value;
    var selectedUsersIds = $('#selectedUserIds').val();
    var data = {
        selectedUserIds: selectedUsersIds.toString(),
        teamName: teamName
    };

    var jsonData = JSON.stringify(data);

    var url = "/Home/AddTeam";
    $.ajax(url,
        {
            type: 'POST',
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: jsonData,
            success: function (recData) {
                if (recData.teamNameErrorMessage != null) {
                    var divElement = document.getElementById('errorView');
                    $(divElement).empty().html(recData.teamNameErrorMessage);
                }
                else if (recData.teamUsersErrorMessage != null) {
                    var divElement = document.getElementById('errorView');
                    $(divElement).empty().html(recData.teamUsersErrorMessage);
                }
                else {
                    window.location.href = recData.redirectToUrl;
                }
            }


        });
}

// Updates the team.

function UpdateTeam(teamId) {
    var teamName = document.getElementById('TeamName').value;
    var selectedUserIds = $('#selectedUserIds').val();
    var data = {
        selectedUserIds: selectedUserIds.toString(),
        teamName: teamName,
        teamId: teamId
    };

    var jsonData = JSON.stringify(data);

    var url = "/Home/UpdateTeam";
    $.ajax(url,
        {
            type: 'POST',
            dataType: 'json',
            contentType: 'application/json; charset= utf-8',
            data: jsonData,

            success: function (recData) {
                if (recData.teamNameErrorMessage != null) {
                    var divElement = document.getElementById('errorView');
                    $(divElement).empty().html(recData.teamNameErrorMessage);
                }
                else if (recData.teamUsersErrorMessage != null) {
                    var divElement = document.getElementById('errorView');
                    $(divElement).empty().html(recData.teamUsersErrorMessage);
                }
                else {
                    window.location.href = recData.redirectToUrl;
                }
            }
        });
}

//Remove Team

function removeTeam(teamId, rowIndex) {
    var url = "/Home/removeTeam?teamId=" + teamId;
    $.ajax(url,
        {
            type: 'DELETE',
            contentType: 'application/json; charset=utf-8',
            success: function (recData) {
                var removedRow = document.getElementById(teamId);
                removedRow.remove();
            }
        });

}

//send test email

function sendTestEmail(teamId, userId) {
    var url = "/Home/SendTestEmail?teamId=" + teamId + "&userId=" + userId;
    $.ajax(url,
        {
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            success: function (recData) {

            }
        });
}

//Remove team member

function removeTeamMember( teamId, userId, rowIndex) {
    var url = "/Home/RemoveTeamMember?teamId=" + teamId + "&userId=" + userId;
    $.ajax(url,
        {
            type:'DELETE',
             contentType: 'application/json; charset=utf-8',
            success: function (recData) {
                var removedRow = document.getElementById(userId);
                removedRow.remove();
            }


        });
}

// Cancel click

function cancel() {
    $('#selecteduserIds').val('');
    window.location.href = '/Home/Index';

}









