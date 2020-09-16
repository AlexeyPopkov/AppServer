import {
  Button,
  ComboBox,
  FieldContainer,
  Icons,
  SearchInput,
  SelectedItem,
  TextInput,
  toastr,
  utils
} from "asc-web-components";
import { PeopleSelector } from "asc-web-common";
import {
  createGroup,
  resetGroup,
  updateGroup
} from "../../../../../store/group/actions";

import { GUID_EMPTY } from "../../../../../helpers/constants";
import PropTypes from "prop-types";
import React from "react";
import { connect } from "react-redux";
import styled from "styled-components";
import { withRouter } from "react-router";
import { withTranslation } from "react-i18next";

const MainContainer = styled.div`
  display: flex;
  flex-direction: column;
  max-width: 1024px;

  .group-name_container {
    max-width: 320px;
  }

  .head_container {
    position: relative;
    max-width: 320px;
  }

  .members_container {
    position: relative;
    max-width: 320px;
    margin: 0;
  }

  .search_container {
    margin-top: 32px;
    display: none;
  }

  .selected-members_container {
    margin-top: 32px;
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    grid-row-gap: 8px;
    grid-column-gap: 16px;
  }

  .buttons_container {
    margin-top: 40px;

    .cancel-button {
      margin-left: 8px;
    }
  }

  @media ${utils.device.tablet} {
    max-width: 320px;

    .selected-members_container {
      grid-template-columns: repeat(1, 1fr);
    }
  }
`;

class SectionBodyContent extends React.Component {
  constructor(props) {
    super(props);
    this.state = this.mapPropsToState();
  }

  mapPropsToState = () => {
    const { group, users, groups, t } = this.props;
    const buttonLabel = group ? t("SaveButton") : t("AddButton");

    const newState = {
      id: group ? group.id : "",
      groupName: group ? group.name : "",
      searchValue: "",
      error: null,
      buttonLabel,
      inLoading: false,
      isHeadSelectorOpen: false,
      isUsersSelectorOpen: false,
      users: users,
      groups: groups,
      header: group
        ? {
            key: 0,
            label: "{SELECTED HEADER NAME}" //group.head
          }
        : {
            key: 0,
            label: t("LblSelect")
          },
      groupMembers:
        group && group.members
          ? group.members.map(m => {
              return {
                key: m.id,
                label: m.displayName
              };
            })
          : [],
      groupManager:
        group && group.manager
          ? {
              key: group.manager.id,
              label: group.manager.displayName === "profile removed" ? t('LblSelect') : group.manager.displayName
            }
          : {
              key: GUID_EMPTY,
              label: t("LblSelect"),
              default: true
            },
      nameError: null
    };

    return newState;
  };

  onGroupChange = e => {
    this.setState({
      groupName: e.target.value
    });
  };

  onSearchChange = value => {
    this.setState({
      searchValue: value
    });
  };

  onHeadSelectorSelect = options => {
    if (!options || !options.length) return;

    const option = options[0];
    this.setState({
      groupManager: {
        key: option.key,
        label: option.label
      },
      isHeadSelectorOpen: !this.state.isHeadSelectorOpen
    });
  };

  onHeadSelectorClick = () => {
    this.setState({
      isHeadSelectorOpen: !this.state.isHeadSelectorOpen
    });
  };

  onUsersSelectorSelect = selectedOptions => {
    //console.log("onSelect", selectedOptions);
    //this.onUsersSelectorClick();
    this.setState({
      groupMembers: selectedOptions.map(option => {
        return {
          key: option.key,
          label: option.label
        };
      }),
      isUsersSelectorOpen: !this.state.isUsersSelectorOpen
    });
  };

  onUsersSelectorClick = () => {
    this.setState({
      isUsersSelectorOpen: !this.state.isUsersSelectorOpen
    });
  };

  save = group => {
    const { createGroup, updateGroup } = this.props;
    return group.id
      ? updateGroup(group.id, group.name, group.managerKey, group.members)
      : createGroup(group.name, group.managerKey, group.members);
  };

  onSave = () => {
    const { group, t, groupCaption, history, settings } = this.props;
    const { groupName, groupManager, groupMembers } = this.state;

    if (!groupName || !groupName.trim().length) {
      this.setState({ nameError: t("EmptyFieldError") });
      return false;
    }

    this.setState({ inLoading: true });

    const newGroup = {
      name: groupName,
      managerKey: groupManager.key,
      members: groupMembers.map(u => u.key)
    };

    if (group && group.id) newGroup.id = group.id;

    this.save(newGroup)
      .then(group => {
        toastr.success(
          t("SuccessSaveGroup", { groupCaption, groupName: group.name })
        );
      })
      .then(() => {
        history.push(`${settings.homepage}/`);
      })
      .catch(error => {
        toastr.error(error);
        this.setState({ inLoading: false });
      });
  };

  onCancel = () => {
    const { history, resetGroup, settings } = this.props;

    resetGroup();
    history.push(`${settings.homepage}/`);
  };

  onSelectedItemClose = member => {
    this.setState({
      groupMembers: this.state.groupMembers.filter(g => g.key !== member.key)
    });
  };

  onCancelSelector = e => {
    if (
      (this.state.isHeadSelectorOpen &&
        (e.target.id === "head-selector_button" ||
          e.target.closest("#head-selector_button"))) ||
      (this.state.isUsersSelectorOpen &&
        (e.target.id === "users-selector_button" ||
          e.target.closest("#users-selector_button")))
    ) {
      // Skip double set of isOpen property
      return;
    }

    this.setState({
      isHeadSelectorOpen: false,
      isUsersSelectorOpen: false
    });
  };

  onKeyPress = event => {
    if (event.key === "Enter") {
      this.onSave();
    }
  };

  onFocusName = () => {
    if (this.state.nameError) this.setState({ nameError: null });
  };

  render() {
    const { t, groupHeadCaption, groupsCaption, me } = this.props;
    const {
      groupName,
      groupMembers,
      isHeadSelectorOpen,
      isUsersSelectorOpen,
      inLoading,
      error,
      searchValue,
      groupManager,
      buttonLabel,
      nameError
    } = this.state;
    return (
      <MainContainer>
        <FieldContainer
          className="group-name_container"
          isRequired={true}
          hasError={!!nameError}
          errorMessage={nameError}
          isVertical={true}
          labelText={t("Name")}
        >
          <TextInput
            id="group-name"
            name="group-name"
            scale={true}
            isAutoFocussed={true}
            isBold={true}
            tabIndex={1}
            value={groupName}
            hasError={!!nameError}
            onChange={this.onGroupChange}
            isDisabled={inLoading}
            onKeyUp={this.onKeyPress}
            onFocus={this.onFocusName}
          />
        </FieldContainer>
        <FieldContainer
          className="head_container"
          isRequired={false}
          hasError={false}
          isVertical={true}
          labelText={groupHeadCaption}
        >
          <ComboBox
            id="head-selector_button"
            tabIndex={2}
            options={[]}
            opened={isHeadSelectorOpen}
            selectedOption={groupManager}
            scaled={true}
            isDisabled={inLoading}
            size="content"
            toggleAction={this.onHeadSelectorClick}
            displayType="toggle"
          >
            <Icons.CatalogGuestIcon size="medium" />
          </ComboBox>
          <PeopleSelector
            isOpen={isHeadSelectorOpen}
            onSelect={this.onHeadSelectorSelect}
            onCancel={this.onCancelSelector}
            groupsCaption={groupsCaption}
            defaultOption={me}
            defaultOptionLabel={t("MeLabel")}
          />
        </FieldContainer>
        <FieldContainer
          className="members_container"
          isRequired={false}
          hasError={false}
          isVertical={true}
          labelText={t("Members")}
        >
          <ComboBox
            id="users-selector_button"
            tabIndex={3}
            options={[]}
            opened={isUsersSelectorOpen}
            isDisabled={inLoading}
            selectedOption={{
              key: 0,
              label: t("AddMembers"),
              default: true
            }}
            scaled={true}
            size="content"
            toggleAction={this.onUsersSelectorClick}
            displayType="toggle"
          >
            <Icons.CatalogGuestIcon size="medium" />
          </ComboBox>
          <PeopleSelector
            isOpen={isUsersSelectorOpen}
            isMultiSelect={true}
            onSelect={this.onUsersSelectorSelect}
            onCancel={this.onCancelSelector}
            searchPlaceHolderLabel={t("SearchAddedMembers")}
            groupsCaption={groupsCaption}
            defaultOption={me}
            defaultOptionLabel={t("MeLabel")}
          />
        </FieldContainer>
        {groupMembers && groupMembers.length > 0 && (
          <>
            <div className="search_container">
              <SearchInput
                id="member-search"
                isDisabled={inLoading}
                scale={true}
                placeholder={t("SearchAddedMembers")}
                value={searchValue}
                onChange={this.onSearchChange}
              />
            </div>
            <div className="selected-members_container">
              {groupMembers.map(member => (
                <SelectedItem
                  key={member.key}
                  text={member.label}
                  onClose={this.onSelectedItemClose.bind(this, member)}
                  isInline={false}
                  className="selected-item"
                  isDisabled={inLoading}
                />
              ))}
            </div>
          </>
        )}
        {error && (
          <div>
            <strong>{error}</strong>
          </div>
        )}
        <div className="buttons_container">
          <Button
            label={buttonLabel}
            primary
            type="submit"
            isLoading={inLoading}
            size="big"
            tabIndex={4}
            onClick={this.onSave}
          />
          <Button
            label={t("CancelButton")}
            className="cancel-button"
            size="big"
            isDisabled={inLoading}
            onClick={this.onCancel}
            tabIndex={5}
          />
        </div>
      </MainContainer>
    );
  }
}

SectionBodyContent.propTypes = {
  group: PropTypes.object
};

SectionBodyContent.defaultProps = {
  group: null
};

const convertUsers = users => {
  return users
    ? users.map(u => {
        return {
          key: u.id,
          groups: u.groups || [],
          label: u.displayName
        };
      })
    : [];
};

const convertGroups = groups => {
  return groups
    ? groups.map(g => {
        return {
          key: g.id,
          label: g.name,
          total: 0
        };
      })
    : [];
};

function mapStateToProps(state) {
  return {
    settings: state.auth.settings,
    group: state.group.targetGroup,
    groups: convertGroups(state.people.groups),
    users: convertUsers(state.people.selector.users), //TODO: replace to api requests with search
    groupHeadCaption: state.auth.settings.customNames.groupHeadCaption,
    groupsCaption: state.auth.settings.customNames.groupsCaption,
    groupCaption: state.auth.settings.customNames.groupCaption,
    me: state.auth.user
  };
}

export default connect(
  mapStateToProps,
  { resetGroup, createGroup, updateGroup }
)(withRouter(withTranslation()(SectionBodyContent)));
