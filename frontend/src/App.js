import './App.css';
import ConfirmEmail from './components/ConfirmEmail';
import EmailForm from './components/EmailForm/EmailForm';
import StatusPage from './components/StatusPage/StatusPage';
import ConfirmUnsubscribe from './components/ConfirmUnsubscribe';
import {useEffect} from 'react';
import {Route, Routes, Navigate, useNavigate} from "react-router-dom";
function App() {
  const navigate = useNavigate();
  useEffect(() => {window.location.pathname === "/" && navigate("/prijava")},[navigate]);
  return (
    <Routes>
      <Route path = "/prijava" element = {<EmailForm/>}/>
      <Route path = "/info" element = {<StatusPage heading = "Kako ovo funkcioniše?" textal = "left" mesg = 
      {<>
      Na servis se prijavljujete <strong>studentskim</strong> mejlom.<br></br>
      Nakon toga, na Vašem mejlu pronađite poruku za potvrdu.<br></br>
      Nakon uspešne potvrde, primaćete notifikacije o izabranim predmetima dok se ne odjavite.<br></br>
      U svakom mejlu koji Vam pošaljemo videćete opciju za odjavljivanje.
      </>
      } pleft = "65px" pright = "35px" hasButton = {true} btnText = "Nazad na formu" fnc = {() => navigate("/prijava")}/>}/>
      <Route path = "/verify" element = {<ConfirmEmail/>}/>
      <Route path = "/unsubscribe" element = {<ConfirmUnsubscribe/>}/>
      <Route path="*" element={<Navigate to="/prijava" />} />
    </Routes>
  );
}

export default App;
