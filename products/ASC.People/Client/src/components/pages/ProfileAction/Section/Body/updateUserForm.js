import React from 'react'
import { withRouter } from 'react-router'
import { connect } from 'react-redux'
import { Avatar, Button, Textarea, Text, toastr, ModalDialog, TextInput } from 'asc-web-components'
import { withTranslation } from 'react-i18next';
import { toEmployeeWrapper, getUserRole, updateProfile } from '../../../../../store/profile/actions';
import { MainContainer, AvatarContainer, MainFieldsContainer } from './FormFields/Form'
import TextField from './FormFields/TextField'
import TextChangeField from './FormFields/TextChangeField'
import DateField from './FormFields/DateField'
import RadioField from './FormFields/RadioField'
import DepartmentField from './FormFields/DepartmentField'
import { department, position, employedSinceDate, typeGuest, typeUser } from '../../../../../helpers/customNames';

class UpdateUserForm extends React.Component {

  constructor(props) {
    super(props);

    this.state = this.mapPropsToState(props);

    this.validate = this.validate.bind(this);
    this.handleSubmit = this.handleSubmit.bind(this);
    this.onInputChange = this.onInputChange.bind(this);
    this.onUserTypeChange = this.onUserTypeChange.bind(this);
    this.onBirthdayDateChange = this.onBirthdayDateChange.bind(this);
    this.onWorkFromDateChange = this.onWorkFromDateChange.bind(this);
    this.onGroupClose = this.onGroupClose.bind(this);
    this.onCancel = this.onCancel.bind(this);

    this.onEmailChange = this.onEmailChange.bind(this);
    this.onSendEmailChangeInstructions = this.onSendEmailChangeInstructions.bind(this);
    this.onPasswordChange = this.onPasswordChange.bind(this);
    this.onSendPasswordChangeInstructions = this.onSendPasswordChangeInstructions.bind(this);
    this.onPhoneChange = this.onPhoneChange.bind(this);
    this.onSendPhoneChangeInstructions = this.onSendPhoneChangeInstructions.bind(this);
    this.onDialogClose = this.onDialogClose.bind(this);
  }

  componentDidUpdate(prevProps, prevState) {
    if (this.props.match.params.userId !== prevProps.match.params.userId) {
      this.setState(this.mapPropsToState(this.props));
    }
  }

  mapPropsToState = (props) => {
    return {
      isLoading: false,
      errors: {
        firstName: false,
        lastName: false,
      },
      profile: toEmployeeWrapper(props.profile),
      dialog: {
        visible: false,
        header: "",
        body: "",
        buttons: [],
        newEmail: "",
      }
    };
  }

  onInputChange(event) {
    var stateCopy = Object.assign({}, this.state);
    stateCopy.profile[event.target.name] = event.target.value;
    this.setState(stateCopy)
  }

  onUserTypeChange(event) {
    var stateCopy = Object.assign({}, this.state);
    stateCopy.profile.isVisitor = event.target.value === "true";
    this.setState(stateCopy)
  }

  onBirthdayDateChange(value) {
    var stateCopy = Object.assign({}, this.state);
    stateCopy.profile.birthday = value ? value.toJSON() : null;
    this.setState(stateCopy)
  }

  onWorkFromDateChange(value) {
    var stateCopy = Object.assign({}, this.state);
    stateCopy.profile.workFrom = value ? value.toJSON() : null;
    this.setState(stateCopy)
  }

  onGroupClose(id) {
    var stateCopy = Object.assign({}, this.state);
    stateCopy.profile.groups = this.state.groups.filter((group) => group.id !== id);
    this.setState(stateCopy)
  }

  validate() {
    const { profile } = this.state;
    const errors = {
      firstName: !profile.firstName,
      lastName: !profile.lastName,
    };
    const hasError = errors.firstName || errors.lastName;
    this.setState({ errors: errors });
    return !hasError;
  }

  handleSubmit() {
    if(!this.validate())
      return false;

    this.setState({isLoading: true});

    this.props.updateProfile(this.state.profile)
      .then((profile) => {
        toastr.success("Success");
        this.props.history.push(`${this.props.settings.homepage}/view/${profile.userName}`);
      })
      .catch((error) => {
        toastr.error(error.message)
        this.setState({isLoading: false})
      });
  }

  onCancel() {
    this.props.history.goBack();
  }

  onEmailChange(event) {
    const dialog = { 
      visible: true,
      header: "Change email",
      body: (
        <Text.Body>
          <span style={{display: "block", marginBottom: "8px"}}>The activation instructions will be sent to the entered email</span>
          <TextInput
            id="new-email"
            scale={true}
            isAutoFocussed={true}
            value={event.target.value}
            onChange={this.onEmailChange}
          />
        </Text.Body>
      ),
      buttons: [
        <Button
          key="SendBtn"
          label="Send"
          size="medium"
          primary={true}
          onClick={this.onSendEmailChangeInstructions}
        />
      ],
      newEmail: event.target.value
     };
    this.setState({ dialog: dialog })
  }

  onSendEmailChangeInstructions() {
    toastr.success("Context action: Change email");
    this.onDialogClose();
  }

  onPasswordChange() {
    const dialog = { 
      visible: true,
      header: "Change password",
      body: (
        <Text.Body>
          Send the password change instructions to the <a href={`mailto:${this.state.profile.email}`}>${this.state.profile.email}</a> email address
        </Text.Body>
      ),
      buttons: [
        <Button
          key="SendBtn"
          label="Send"
          size="medium"
          primary={true}
          onClick={this.onSendPasswordChangeInstructions}
        />
      ]
     };
    this.setState({ dialog: dialog })
  }

  onSendPasswordChangeInstructions() {
    toastr.success("Context action: Change password");
    this.onDialogClose();
  }

  onPhoneChange() {
    const dialog = { 
      visible: true,
      header: "Change phone",
      body: (
        <Text.Body>
          The instructions on how to change the user mobile number will be sent to the user email address
        </Text.Body>
      ),
      buttons: [
        <Button
          key="SendBtn"
          label="Send"
          size="medium"
          primary={true}
          onClick={this.onSendPhoneChangeInstructions}
        />
      ]
     };
    this.setState({ dialog: dialog })
  }

  onSendPhoneChangeInstructions() {
    toastr.success("Context action: Change phone");
    this.onDialogClose();
  }

  onDialogClose() {
    const dialog = { visible: false }; 
    this.setState({ dialog: dialog })
  }

  render() {
    const { isLoading, errors, profile, dialog } = this.state;
    const { t } = this.props;

    return (
      <>
        <MainContainer>
          <AvatarContainer>
            <Avatar
              size="max"
              role={getUserRole(profile)}
              source={profile.avatarMax}
              userName={profile.displayName}
              editing={true}
              editLabel={t("EditPhoto")}
            />
          </AvatarContainer>
          <MainFieldsContainer>
            <TextChangeField
              labelText={`${t("Email")}:`}
              inputName="email"
              inputValue={profile.email}
              buttonText={t("ChangeButton")}
              buttonIsDisabled={isLoading}
              buttonOnClick={this.onEmailChange}
              buttonTabIndex={1}
            />
            <TextChangeField
              labelText={`${t("Password")}:`}
              inputName="password"
              inputValue={profile.password}
              buttonText={t("ChangeButton")}
              buttonIsDisabled={isLoading}
              buttonOnClick={this.onPasswordChange}
              buttonTabIndex={2}
            />
            <TextChangeField
              labelText={`${t("Phone")}:`}
              inputName="phone"
              inputValue={profile.phone}
              buttonText={t("ChangeButton")}
              buttonIsDisabled={isLoading}
              buttonOnClick={this.onPhoneChange}
              buttonTabIndex={3}
            />
            <TextField
              isRequired={true}
              hasError={errors.firstName}
              labelText={`${t("FirstName")}:`}
              inputName="firstName"
              inputValue={profile.firstName}
              inputIsDisabled={isLoading}
              inputOnChange={this.onInputChange}
              inputAutoFocussed={true}
              inputTabIndex={4}
            />
            <TextField
              isRequired={true}
              hasError={errors.lastName}
              labelText={`${t("LastName")}:`}
              inputName="lastName"
              inputValue={profile.lastName}
              inputIsDisabled={isLoading}
              inputOnChange={this.onInputChange}
              inputTabIndex={5}
            />
            <DateField
              labelText={`${t("Birthdate")}:`}
              inputName="birthday"
              inputValue={profile.birthday ? new Date(profile.birthday) : undefined}
              inputIsDisabled={isLoading}
              inputOnChange={this.onBirthdayDateChange}
              inputTabIndex={6}
            />
            <RadioField
              labelText={`${t("Sex")}:`}
              radioName="sex"
              radioValue={profile.sex}
              radioOptions={[
                { value: 'male', label: t("SexMale")},
                { value: 'female', label: t("SexFemale")}
              ]}
              radioIsDisabled={isLoading}
              radioOnChange={this.onInputChange}
            />
            <RadioField
              labelText={`${t("UserType")}:`}
              radioName="isVisitor"
              radioValue={profile.isVisitor.toString()}
              radioOptions={[
                { value: "true", label: t("CustomTypeGuest", { typeGuest })},
                { value: "false", label: t("CustomTypeUser", { typeUser })}
              ]}
              radioIsDisabled={isLoading}
              radioOnChange={this.onUserTypeChange}
            />
            <DateField
              labelText={`${t("CustomEmployedSinceDate", { employedSinceDate })}:`}
              inputName="workFrom"
              inputValue={profile.workFrom ? new Date(profile.workFrom) : undefined}
              inputIsDisabled={isLoading}
              inputOnChange={this.onWorkFromDateChange}
              inputTabIndex={7}
            />
            <TextField
              labelText={`${t("Location")}:`}
              inputName="location"
              inputValue={profile.location}
              inputIsDisabled={isLoading}
              inputOnChange={this.onInputChange}
              inputTabIndex={8}
            />
            <TextField
              labelText={`${t("CustomPosition", { position })}:`}
              inputName="title"
              inputValue={profile.title}
              inputIsDisabled={isLoading}
              inputOnChange={this.onInputChange}
              inputTabIndex={9}
            />
            <DepartmentField
              labelText={`${t("CustomDepartment", { department })}:`}
              departments={profile.groups}
              onRemoveDepartment={this.onGroupClose}
            />
          </MainFieldsContainer>
        </MainContainer>
        <div>
          <Text.ContentHeader>{t("Comments")}</Text.ContentHeader>
          <Textarea name="notes" value={profile.notes} isDisabled={isLoading} onChange={this.onInputChange} tabIndex={10}/> 
        </div>
        <div style={{marginTop: "60px"}}>
          <Button label={t("SaveButton")} onClick={this.handleSubmit} primary isDisabled={isLoading} size="big" tabIndex={11}/>
          <Button label={t("CancelButton")} onClick={this.onCancel} isDisabled={isLoading} size="big" style={{ marginLeft: "8px" }} tabIndex={12}/>
        </div>

        <ModalDialog
          visible={dialog.visible}
          headerContent={dialog.header}
          bodyContent={dialog.body}
          footerContent={dialog.buttons}
          onClose={this.onDialogClose}
        />
      </>
    );
  };
}

const mapStateToProps = (state) => {
  return {
    profile: state.profile.targetUser,
    settings: state.auth.settings
  }
};

export default connect(
  mapStateToProps,
  {
    updateProfile
  }
)(withRouter(withTranslation()(UpdateUserForm)));